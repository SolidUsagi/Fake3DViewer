using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Video;
using UnityEngine;

public abstract class RgbdImageProvider : MonoBehaviour
{
	public VideoPlayer videoPlayer;

	public enum RgbdImageProviderEventType
	{
		StartFadeIn,
		StartFadeOut,
		OneCycleOfSlideShowEnded
	}

	public delegate void RgbdImageProviderEvent(RgbdImageProviderEventType rgbdImageProviderEventType);
	public event RgbdImageProviderEvent OnRgbdImageProviderEvent;

	protected const int rgbdImageTextureWidth  = 2048;
	protected const int rgbdImageTextureHeight = 2048;

	private bool dirty = false;
	private bool play  = false;

	public enum MediaType
	{
		Image,
		Video
	}

	private MediaType sourceMediaType = MediaType.Image;
	public  MediaType SourceMediaType
	{
		set { this.sourceMediaType = value; dirty = true; }
		get { return this.sourceMediaType;                }
	}

	public enum ImageArrangement
	{
		LR,
		RL,
		TB,
		BT,
		Separate
	}

	private ImageArrangement sourceImageArrangement = ImageArrangement.LR;
	public  ImageArrangement SourceImageArrangement
	{
		set { this.sourceImageArrangement = value; dirty = true; }
		get { return this.sourceImageArrangement;                }
	}

	private string imageFilePath = "";
	public  string ImageFilePath
	{
		set { this.imageFilePath = value; dirty = true; }
		get { return this.imageFilePath;                }
	}

	private string rgbImageFilePath = "";
	public  string RgbImageFilePath
	{
		set { this.rgbImageFilePath = value; dirty = true; }
		get { return this.rgbImageFilePath;                }
	}

	private string depthImageFilePath = "";
	public  string DepthImageFilePath
	{
		set { this.depthImageFilePath = value; dirty = true; }
		get { return this.depthImageFilePath;                }
	}

	private Texture2D      imageTexture = null;
	private Texture2D   rgbImageTexture = null;
	private Texture2D depthImageTexture = null;

	private string videoFilePath = "";
	public  string VideoFilePath
	{
		set { this.videoFilePath = value; dirty = true; play = true; }
		get { return this.videoFilePath;                             }
	}

	public enum ZAxisOrientation
	{
		FrontPositive,
		FrontNegative
	}

	private ZAxisOrientation depthOrientation = ZAxisOrientation.FrontPositive;
	public  ZAxisOrientation DepthOrientation
	{
		set { this.depthOrientation = value; dirty = true; }
		get { return this.depthOrientation;                }
	}

	private bool noError = true;
	public  bool NoError
	{
		get { bool noError = this.noError; this.noError = true; return noError; }
	}

	protected int textureWidth  = 0;
	protected int textureHeight = 0;

	public int ImageWidth
	{
		get
		{
			switch (sourceImageArrangement)
			{
				default:                        return textureWidth / 2;
				case ImageArrangement.RL:       return textureWidth / 2;
				case ImageArrangement.TB:       return textureWidth;
				case ImageArrangement.BT:       return textureWidth;
				case ImageArrangement.Separate: return textureWidth;
			}
		}
	}

	public int ImageHeight
	{
		get
		{
			switch (sourceImageArrangement)
			{
				default:                        return textureHeight;
				case ImageArrangement.RL:       return textureHeight;
				case ImageArrangement.TB:       return textureHeight / 2;
				case ImageArrangement.BT:       return textureHeight / 2;
				case ImageArrangement.Separate: return textureHeight;
			}
		}
	}

	public enum CameraPosition
	{
		Initial,
		Current,
		Random
	}

	private bool moving  = false;
	private bool resting = false;
	private bool fading  = false;

	private float movingPeriod  = 0.0f;
	private float elapsedTime   = 0.0f;
	private float remainingTime = 0.0f;

	private const float ResetAnimationPeriod = 0.5f;

	private bool  slideShow               = false;
	private float slideShowOneCyclePeriod = 0.0f;
	private float slideShowRestPeriod     = 0.0f;
	private float slideShowFadeOutPeriod  = 0.0f;

	void Start()
	{
		videoPlayer.prepareCompleted += (VideoPlayer videoPlayer) =>
		{
			textureWidth  = videoPlayer.texture.width;
			textureHeight = videoPlayer.texture.height;

			RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);

			videoPlayer.targetTexture = renderTexture;

			Material material = GetMaterial();
			if (material.HasProperty("_MainTex"))
			{
				material.SetTexture("_MainTex", renderTexture);
			}

			OnVideoPlayerPrepareCompleted();

			videoPlayer.Play();

			if (OnRgbdImageProviderEvent != null)
			{
				OnRgbdImageProviderEvent(RgbdImageProviderEventType.StartFadeIn);
			}
		};

		videoPlayer.loopPointReached += (VideoPlayer videoPlayer) =>
		{
			if (slideShow && !fading)
			{
				fading        = true;
				remainingTime = slideShowFadeOutPeriod;

				OnRgbdImageProviderEvent(RgbdImageProviderEventType.StartFadeOut);
			}
		};

		videoPlayer.errorReceived += (VideoPlayer videoPlayer, string message) =>
		{
			noError = false;
		};

		OnStart();
	}

	void Update()
	{
		if (dirty)
		{
			noError = true;

			textureWidth  = 0;
			textureHeight = 0;

			Material material = GetMaterial();
			material.shader = Shader.Find(GetSuitableShaderName(sourceImageArrangement));

			if (sourceMediaType == MediaType.Image)
			{
				if (sourceImageArrangement != ImageArrangement.Separate)
				{
					imageTexture = MakeTexture2DFromImageFile(imageFilePath);
					if (imageTexture != null && material.HasProperty("_MainTex"))
					{
						material.SetTexture("_MainTex", imageTexture);

						textureWidth  = imageTexture.width;
						textureHeight = imageTexture.height;
					}
					else
					{
						noError = false;
					}
				}
				else
				{
					rgbImageTexture = MakeTexture2DFromImageFile(rgbImageFilePath);
					if (rgbImageTexture != null && material.HasProperty("_MainTex"))
					{
						material.SetTexture("_MainTex", rgbImageTexture);

						textureWidth  = rgbImageTexture.width;
						textureHeight = rgbImageTexture.height;
					}
					else
					{
						noError = false;
					}

					depthImageTexture = MakeTexture2DFromImageFile(depthImageFilePath);
					if (depthImageTexture != null && material.HasProperty("_DepthMap"))
					{
						material.SetTexture("_DepthMap", depthImageTexture);
					}
					else
					{
						noError = false;
					}
				}

				if (noError)
				{
					OnSetTextureToShader();
				}
			}
			else
			{
				if (play)
				{
					videoPlayer.url = videoFilePath;
					videoPlayer.Prepare();
				}
			}

			float depthTop    = 1.0f;
			float depthBottom = 0.0f;
			if (depthOrientation != ZAxisOrientation.FrontPositive)
			{
				depthTop    = 0.0f;
				depthBottom = 1.0f;
			}
			if (material.HasProperty("_DepthTop"   ))
			{
				material.SetFloat("_DepthTop",    depthTop   );
			}
			if (material.HasProperty("_DepthBottom"))
			{
				material.SetFloat("_DepthBottom", depthBottom);
			}

			if (sourceMediaType == MediaType.Image && OnRgbdImageProviderEvent != null)
			{
				OnRgbdImageProviderEvent(RgbdImageProviderEventType.StartFadeIn);
			}
		}

		dirty = false;

		  elapsedTime += Time.deltaTime;
		remainingTime -= Time.deltaTime;

		if (moving)
		{
			if (!slideShow)
			{
				moving = elapsedTime <= movingPeriod;
			}

			float ratio;
			if (!(slideShow && sourceMediaType == MediaType.Video))
			{
				ratio = elapsedTime / movingPeriod;
			}
			else
			{
				ratio = -0.5f * (Mathf.Cos(Mathf.PI * elapsedTime / movingPeriod) - 1.0f);
			}

			MoveCamera(System.Math.Min(ratio, 1.0f));
		}

		if (slideShow)
		{
			if (moving)
			{
				if (elapsedTime > movingPeriod)
				{
					moving = false;

					if (sourceMediaType == MediaType.Video)
					{
						resting     = true;
						elapsedTime = 0.0f;
					}
				}
			}
			else if (resting)
			{
				if (elapsedTime > slideShowRestPeriod)
				{
					resting = false;

					PrepareCameraMoving(CameraPosition.Current, CameraPosition.Random);

					moving       = true;
					movingPeriod = slideShowOneCyclePeriod;
					elapsedTime  = 0.0f;
				}
			}

			if (sourceMediaType == MediaType.Image && !fading && remainingTime <= slideShowFadeOutPeriod)
			{
				fading = true;

				OnRgbdImageProviderEvent(RgbdImageProviderEventType.StartFadeOut);
			}

			if (fading && remainingTime < 0.0f)
			{
				slideShow = false;

				OnRgbdImageProviderEvent(RgbdImageProviderEventType.OneCycleOfSlideShowEnded);
			}
		}

		OnUpdate();
	}

	private void OnDisable()
	{
		Material material = GetMaterial();

		if (material.HasProperty("_MainTex"))
		{
			material.SetTexture("_MainTex", null);
		}

		if (material.HasProperty("_DepthMap"))
		{
			material.SetTexture("_DepthMap", null);
		}

		if (imageTexture != null)
		{
			Destroy(imageTexture);
			imageTexture = null;
		}

		if (rgbImageTexture != null)
		{
			Destroy(rgbImageTexture);
			rgbImageTexture = null;
		}

		if (depthImageTexture != null)
		{
			Destroy(depthImageTexture);
			depthImageTexture = null;
		}

		if (videoPlayer.targetTexture != null)
		{
			Destroy(videoPlayer.targetTexture);
			videoPlayer.targetTexture = null;
		}
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnSetTextureToShader()
	{
	}

	protected virtual void OnVideoPlayerPrepareCompleted()
	{
	}

	public virtual void OnDrag(Vector3 delta)
	{
	}

	public virtual void OnWheel(float delta)
	{
	}

	protected virtual void ResetCamera()
	{
	}

	protected virtual void PrepareCameraMoving(CameraPosition from, CameraPosition to)
	{
	}

	protected virtual void MoveCamera(float ratio)
	{
	}

	public abstract Texture GetRgbdImageTexture();

	protected abstract Material GetMaterial();

	protected abstract string GetSuitableShaderName(ImageArrangement imageArrangement);

	public void OnClick(int clickCount)
	{
		if (clickCount == 2 && !slideShow && !moving)
		{
			PrepareCameraMoving(CameraPosition.Current, CameraPosition.Initial);

			moving       = true;
			movingPeriod = ResetAnimationPeriod;
			elapsedTime  = 0.0f;
		}
	}

	public void InitState()
	{
		ResetCamera();

		moving    = false;
		resting   = false;
		fading    = false;
		slideShow = false;
	}

	public void StartOneCycleOfSlideShow(float oneCyclePeriod, float restPeriod, float fadeOutPeriod)
	{
		slideShow               = true;
		slideShowOneCyclePeriod = oneCyclePeriod;
		slideShowRestPeriod     = restPeriod;
		slideShowFadeOutPeriod  = fadeOutPeriod;

		PrepareCameraMoving(CameraPosition.Random, CameraPosition.Random);

		moving        = true;
		movingPeriod  = slideShowOneCyclePeriod;
		elapsedTime   = 0.0f;
		remainingTime = sourceMediaType == MediaType.Image ? slideShowOneCyclePeriod : 0.0f;
	}

	private Texture2D MakeTexture2DFromImageFile(string filePath)
	{
		try
		{
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (BinaryReader binaryReader = new BinaryReader(fileStream))
			{
				Texture2D texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(binaryReader.ReadBytes((int)binaryReader.BaseStream.Length));

				return texture2D;
			}
		}
		catch (IOException)
		{
			return null;
		}
	}
}
