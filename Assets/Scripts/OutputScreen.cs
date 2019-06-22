using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputScreen : MonoBehaviour
{
	public enum OutputScreenEventType
	{
		PositionUpdated,
		ScaleUpdated,
		DepthUpdated
	}

	public delegate void OutputScreenEvent(OutputScreenEventType outputScreenEventType);
	public event OutputScreenEvent OnOutputScreenEvent;

	private Material material = null;

	private enum ShaderType
	{
		Parallax,
		DepthOnly,
		RgbOnly
	}

	private ShaderType shaderType = ShaderType.Parallax;

	public const float Position_Initial =  -5.0f;
	public const float Position_Lower   = -15.0f;
	public const float Position_Upper   =   5.0f;

	public float Position
	{
		set { transform.localPosition = new Vector3(0.0f, 0.0f, value); }
		get { return transform.localPosition.z;                         }
	}

	public const float Scale_Initial = 18.0f;
	public const float Scale_Lower   =  8.0f;
	public const float Scale_Upper   = 28.0f;

	public float Scale
	{
		set { transform.localScale = new Vector3(value, value, 1.0f); }
		get { return transform.localScale.x;                          }
	}

	public const float Depth_Initial = 10.0f;
	public const float Depth_Lower   =  1.0f;
	public const float Depth_Upper   = 20.0f;

	private float depth = Depth_Initial;
	public  float Depth
	{
		set { this.depth = value; dirty = true; }
		get { return this.depth;                }
	}

	private const int depthDivisor = 64;

	private float colorIntensity = 1.0f;

	private bool dirty = true;

	private float elapsedTime_Position = 0.0f;
	private float elapsedTime_Scale    = 0.0f;
	private float elapsedTime_Depth    = 0.0f;
	private float elapsedTime_Tilt     = 0.0f;
	private float elapsedTime_Fade     = 0.0f;
	private const float ResetAnimationPeriod = 0.2f;

	private bool  positionResetting = false;
	private float position_From     = Position_Initial;

	private bool  scaleResetting = false;
	private float scale_From     = Scale_Initial;

	private bool  depthResetting = false;
	private float depth_From     = Depth_Initial;

	private bool       tiltResetting = false;
	private Quaternion tilt_From     = Quaternion.identity;

	private bool  fadeIn     = false;
	private bool  fadeOut    = false;
	private float fadePeriod = 0.0f;

	private float mouseMovingRatio = 1.0f;

	void Start()
	{
		material = GetComponent<Renderer>().material;
		material.shader = Shader.Find("OutputScreen/Parallax");

		transform.localPosition = new Vector3(0.0f, 0.0f, Position_Initial);
		transform.localScale    = new Vector3(Scale_Initial, Scale_Initial, 1.0f);

		mouseMovingRatio = 30.0f / Screen.currentResolution.height;
	}

	void Update()
	{
		elapsedTime_Position += Time.deltaTime;

		if (positionResetting)
		{
			positionResetting = elapsedTime_Position <= ResetAnimationPeriod;

			float ratio = System.Math.Min(elapsedTime_Position / ResetAnimationPeriod, 1.0f);
			transform.localPosition = new Vector3(0.0f, 0.0f, Mathf.Lerp(position_From, Position_Initial, ratio));

			if (!positionResetting && OnOutputScreenEvent != null)
			{
				OnOutputScreenEvent(OutputScreenEventType.PositionUpdated);
			}
		}

		elapsedTime_Scale += Time.deltaTime;

		if (scaleResetting)
		{
			scaleResetting = elapsedTime_Scale <= ResetAnimationPeriod;

			float ratio = System.Math.Min(elapsedTime_Scale / ResetAnimationPeriod, 1.0f);
			float scale = Mathf.Lerp(scale_From, Scale_Initial, ratio);
			transform.localScale = new Vector3(scale, scale, 1.0f);

			if (!scaleResetting && OnOutputScreenEvent != null)
			{
				OnOutputScreenEvent(OutputScreenEventType.ScaleUpdated);
			}
		}

		elapsedTime_Depth += Time.deltaTime;

		if (depthResetting)
		{
			depthResetting = elapsedTime_Depth <= ResetAnimationPeriod;

			float ratio = System.Math.Min(elapsedTime_Depth / ResetAnimationPeriod, 1.0f);
			depth = Mathf.Lerp(depth_From, Depth_Initial, ratio);
			dirty = true;

			if (!depthResetting && OnOutputScreenEvent != null)
			{
				OnOutputScreenEvent(OutputScreenEventType.DepthUpdated);
			}
		}

		elapsedTime_Tilt += Time.deltaTime;

		if (tiltResetting)
		{
			tiltResetting = elapsedTime_Tilt <= ResetAnimationPeriod;

			float ratio = System.Math.Min(elapsedTime_Tilt / ResetAnimationPeriod, 1.0f);
			transform.rotation = Quaternion.Slerp(tilt_From, Quaternion.identity, ratio);
		}

		elapsedTime_Fade += Time.deltaTime;

		if (fadeIn || fadeOut)
		{
			colorIntensity = System.Math.Min(elapsedTime_Fade / fadePeriod, 1.0f);
			if (fadeOut)
			{
				colorIntensity = 1.0f - colorIntensity;
			}

			fadeIn  = fadeIn  ? elapsedTime_Fade <= fadePeriod : false;
			fadeOut = fadeOut ? elapsedTime_Fade <= fadePeriod : false;
			dirty   = true;
		}

		if (Input.GetKeyDown(KeyCode.F12))
		{
			if (!positionResetting && !scaleResetting && !depthResetting)
			{
				if (shaderType != ShaderType.DepthOnly)
				{
					material.shader = Shader.Find("OutputScreen/DepthOnly");
					shaderType = ShaderType.DepthOnly;
				}
				else
				{
					material.shader = Shader.Find("OutputScreen/Parallax");
					shaderType = ShaderType.Parallax;
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.F11))
		{
			if (!positionResetting && !scaleResetting && !depthResetting)
			{
				if (shaderType != ShaderType.RgbOnly)
				{
					material.shader = Shader.Find("OutputScreen/RgbOnly");
					shaderType = ShaderType.RgbOnly;
				}
				else
				{
					material.shader = Shader.Find("OutputScreen/Parallax");
					shaderType = ShaderType.Parallax;
				}
			}
		}

		if (dirty)
		{
			if (material.HasProperty("_DeepestDepth"))
			{
				material.SetFloat("_DeepestDepth", depth);
			}

			if (material.HasProperty("_StepLength"))
			{
				material.SetFloat("_StepLength", depth / depthDivisor);
			}

			if (material.HasProperty("_ColorIntensity"))
			{
				material.SetFloat("_ColorIntensity", colorIntensity);
			}
		}

		dirty = false;
	}

	public void SetRgbdImageTexture(Texture texture)
	{
		if (material.HasProperty("_MainTex"))
		{
			material.SetTexture("_MainTex", texture);
		}
	}

	public void OnDrag(Vector3 delta)
	{
		Vector3 eulerAngles = transform.rotation.eulerAngles;
		eulerAngles.x = eulerAngles.x < 180.0f ? eulerAngles.x : eulerAngles.x - 360.0f;
		eulerAngles.y = eulerAngles.y < 180.0f ? eulerAngles.y : eulerAngles.y - 360.0f;

		float yaw   = 0.0f;
		float pitch = 0.0f;
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			yaw   = -delta.x * mouseMovingRatio; yaw   = yaw   >= 0.0f ? System.Math.Min(yaw,   15.0f - eulerAngles.y) : System.Math.Max(yaw,   -15.0f - eulerAngles.y);
		}
		if (Input.GetKey(KeyCode.LeftShift  ) || Input.GetKey(KeyCode.RightShift  ))
		{
			pitch =  delta.y * mouseMovingRatio; pitch = pitch >= 0.0f ? System.Math.Min(pitch, 15.0f - eulerAngles.x) : System.Math.Max(pitch, -15.0f - eulerAngles.x);
		}

		Quaternion rotation = Quaternion.AngleAxis(yaw,     Vector3.up   )
							* Quaternion.AngleAxis(pitch, transform.right)
							* transform.rotation;

		transform.rotation = System.Math.Abs(rotation.eulerAngles.z) < 1.0f ? rotation : transform.rotation;
	}

	public void OnButtonUp()
	{
		elapsedTime_Tilt = 0.0f;
		tiltResetting    = true;
		tilt_From        = transform.rotation;
	}

	public void OnWheel(float delta)
	{
		if (Input.GetKey(KeyCode.Z))
		{
			if (!positionResetting)
			{
				float z = transform.localPosition.z - delta;
				z = System.Math.Max(Position_Lower, System.Math.Min(z, Position_Upper));

				transform.localPosition = new Vector3(0.0f, 0.0f, z);

				if (OnOutputScreenEvent != null)
				{
					OnOutputScreenEvent(OutputScreenEventType.PositionUpdated);
				}
			}
		}
		else if (Input.GetKey(KeyCode.S))
		{
			if (!scaleResetting)
			{
				float scale = transform.localScale.x + delta;
				scale = System.Math.Max(Scale_Lower, System.Math.Min(scale, Scale_Upper));

				transform.localScale = new Vector3(scale, scale, 1.0f);

				if (OnOutputScreenEvent != null)
				{
					OnOutputScreenEvent(OutputScreenEventType.ScaleUpdated);
				}
			}
		}
		else if (Input.GetKey(KeyCode.D))
		{
			if (!depthResetting && shaderType == ShaderType.Parallax)
			{
				depth -= delta;
				depth = System.Math.Max(Depth_Lower, System.Math.Min(depth, Depth_Upper));

				dirty = true;

				if (OnOutputScreenEvent != null)
				{
					OnOutputScreenEvent(OutputScreenEventType.DepthUpdated);
				}
			}
		}
	}

	public void OnClick(int clickCount)
	{
		if (clickCount == 2)
		{
			if (Input.GetKey(KeyCode.Z))
			{
				if (!positionResetting)
				{
					elapsedTime_Position = 0.0f;
					positionResetting    = true;
					position_From        = transform.localPosition.z;
				}
			}
			else if (Input.GetKey(KeyCode.S))
			{
				if (!scaleResetting)
				{
					elapsedTime_Scale = 0.0f;
					scaleResetting    = true;
					scale_From        = transform.localScale.x;
				}
			}
			else if (Input.GetKey(KeyCode.D))
			{
				if (!depthResetting && shaderType == ShaderType.Parallax)
				{
					elapsedTime_Depth = 0.0f;
					depthResetting    = true;
					depth_From        = depth;
				}
			}
		}
	}

	public void FadeIn(float period)
	{
		elapsedTime_Fade = 0.0f;
		fadeIn           = true;
		fadeOut          = false;
		fadePeriod       = period;
	}

	public void FadeOut(float period)
	{
		elapsedTime_Fade = 0.0f;
		fadeIn           = false;
		fadeOut          = true;
		fadePeriod       = period;
	}

	public void Darken()
	{
		colorIntensity = 0.0f;
		fadeIn         = false;
		fadeOut        = false;
		dirty          = true;

		if (material.HasProperty("_ColorIntensity"))
		{
			material.SetFloat("_ColorIntensity", colorIntensity);
		}
	}
}
