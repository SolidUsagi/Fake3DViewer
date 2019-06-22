using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using B83.Win32;
using LookingGlass;

public class MainController : MonoBehaviour
{
	public EquirectangularImage equirectangularImage;
	public RectangularImage     rectangularImage;
	public OutputScreen         outputScreen;

	private RgbdImageProvider rgbdImageProvider = null;

	public class Defaults
	{
		public List<List<string>> Exhibits = new List<List<string>>();
	}

	private Defaults defaults         = null;
	private string   defaultsFilePath = "";

	private List<string> presentingExhibit = null;
	private bool            droppedExhibit = false;

	private enum SourceImageProjection
	{
		EquirectangularHalf,
		EquirectangularFull,
		Rectangular
	}

#if !UNITY_EDITOR
	private string startImageFilePath = Application.streamingAssetsPath + "/Images/Start_LR.jpg";
#else
	private string startImageFilePath = Application.streamingAssetsPath + "/Images/UnityEditor_LR.jpg";
#endif

	private const string EquirectangularHalf = "180";
	private const string EquirectangularFull = "360";
	private const string Rectangular         = "RECTANGLE";
	private const string LR                  = "LR";
	private const string RL                  = "RL";
	private const string TB                  = "TB";
	private const string BT                  = "BT";
	private const string FrontNegative       = "INV";

	private float elapsedTime = 0.0f;
	private const float     ClickAcceptancePeriod = 0.3f;
	private const float NextClickAcceptancePeriod = 0.2f;

	private bool buttonDowned = false;
	private int  clickCount   = 0;

	private Vector3 previousMousePosition = Vector3.zero;

	private bool tiltOutputScreen = false;

	private const float FadeInPeriod = 0.5f;

	private bool slideShow = false;

	private const float SlideShowOneCyclePeriod = 8.0f;
	private const float SlideShowRestPeriod     = 0.2f;
	private const float SlideShowFadeOutPeriod  = 2.0f;

	void Start()
	{
		equirectangularImage.OnRgbdImageProviderEvent += OnRgbdImageProviderEvent;
		    rectangularImage.OnRgbdImageProviderEvent += OnRgbdImageProviderEvent;

		outputScreen.OnOutputScreenEvent += (OutputScreen.OutputScreenEventType outputScreenEventType) =>
		{
			UpdateConfig2();
		};

		defaultsFilePath = Application.persistentDataPath + "/Defaults.xml";

		try
		{
			using (FileStream fileStream = new FileStream(defaultsFilePath, FileMode.Open, FileAccess.Read))
			{
				defaults = (Defaults)(new System.Xml.Serialization.XmlSerializer(typeof(Defaults))).Deserialize(fileStream);
				if (defaults == null)
				{
					defaults = new Defaults();
				}
			}
		}
		catch (IOException)
		{
			defaults = new Defaults();
		}

		if (defaults.Exhibits.Count == 0)
		{
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu1_180LR.jpg", "180LR", "" });
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu2_180LR.jpg", "180LR", "" });
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu3_180LR.jpg", "180LR", "" });
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu4_180LR.jpg", "180LR", "" });
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu5_180LR.jpg", "180LR", "" });
			defaults.Exhibits.Add(new List<string> { Application.streamingAssetsPath + "/Images/Fuyu6_180LR.jpg", "180LR", "" });
		}

		Random.InitState(System.DateTime.Now.Millisecond);

		Present(new List<string>() { startImageFilePath });
	}

	void Update()
	{
		if (!rgbdImageProvider.NoError)
		{
			RemovePresentingExhibit(droppedExhibit);
		}

		if (!slideShow)
		{
			if ((Input.GetKeyDown(KeyCode.Space) || ButtonManager.GetButtonDown(ButtonType.CIRCLE)) && defaults.Exhibits.Count > 0)
			{
				slideShow = true;

				Present(0);
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Space)     || Input.GetKeyDown(KeyCode.Escape)     ||
				Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
				Input.GetMouseButtonDown(0)         || Input.GetMouseButtonDown(1)          || ButtonManager.GetAnyButtonDown())
			{
				slideShow = false;

				Present(new List<string>() { startImageFilePath });

				previousMousePosition = Input.mousePosition;

				return;
			}
		}

		if (slideShow)
		{
			return;
		}

		elapsedTime += Time.deltaTime;

		if (Input.GetMouseButtonDown(0))
		{
			buttonDowned = true;
			clickCount   = clickCount > 0 && elapsedTime <= NextClickAcceptancePeriod ? clickCount : 0;
			elapsedTime  = 0.0f;

			previousMousePosition = Input.mousePosition;

			if (Input.GetKey(KeyCode.LeftShift  ) || Input.GetKey(KeyCode.RightShift  ) ||
				Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				tiltOutputScreen = true;
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			buttonDowned = false;
			clickCount   = elapsedTime <= ClickAcceptancePeriod ? clickCount + 1 : 0;
			elapsedTime  = 0.0f;

			if (tiltOutputScreen)
			{
				outputScreen.OnButtonUp();
			}

			tiltOutputScreen = false;
		}

		if (!buttonDowned && clickCount > 0 && elapsedTime > NextClickAcceptancePeriod)
		{
			if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
			{
				rgbdImageProvider.OnClick(clickCount);
			}
			else
			{
				     outputScreen.OnClick(clickCount);
			}

			clickCount = 0;
		}

		if (Input.GetMouseButton(0))
		{
			Vector3 delta = Input.mousePosition - previousMousePosition;

			if (System.Math.Abs(delta.x) > 1e-8 || System.Math.Abs(delta.y) > 1e-8)
			{
				previousMousePosition = Input.mousePosition;

				if (!tiltOutputScreen)
				{
					rgbdImageProvider.OnDrag(delta);
				}
				else
				{
					     outputScreen.OnDrag(delta);
				}
			}
		}

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (System.Math.Abs(scrollWheel) > 1e-8)
		{
			if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
			{
				rgbdImageProvider.OnWheel(scrollWheel);
			}
			else
			{
				     outputScreen.OnWheel(scrollWheel);
			}
		}

		if (Input.GetKeyDown(KeyCode.F1))
		{
			ChangeDepthOrientation();
		}

		if (Input.GetKeyDown(KeyCode.F2))
		{
			ChangeSourceImageProjection();
		}

		if (Input.GetKeyDown(KeyCode.F3))
		{
			ChangeSourceImageArrangementDirection();
		}

		if (Input.GetKeyDown(KeyCode.F4))
		{
			ChangeSourceImageArrangementOrder();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow ) || ButtonManager.GetButtonDown(ButtonType.LEFT  ))
		{
			ToPrevExhibit();
		}

		if (Input.GetKeyDown(KeyCode.RightArrow) || ButtonManager.GetButtonDown(ButtonType.RIGHT ))
		{
			ToNextExhibit();
		}

		if (Input.GetKeyDown(KeyCode.Delete    ))
		{
			RemovePresentingExhibit();
		}

		if (Input.GetKeyDown(KeyCode.Escape    ) || ButtonManager.GetButtonDown(ButtonType.SQUARE))
		{
			Present(new List<string>() { startImageFilePath });
		}
	}

	private void OnDestroy()
	{
#if !UNITY_EDITOR
		try
		{
			using (FileStream fileStream = new FileStream(defaultsFilePath, FileMode.Create, FileAccess.Write))
			{
				(new System.Xml.Serialization.XmlSerializer(typeof(Defaults))).Serialize(fileStream, defaults);
			}
		}
		catch (IOException)
		{
		}
#endif
	}

	private UnityDragAndDropHook unityDragAndDropHook;

	private void OnEnable()
	{
		unityDragAndDropHook = new UnityDragAndDropHook();
		unityDragAndDropHook.InstallHook();
		unityDragAndDropHook.OnDroppedFiles += OnDroppedFiles;
	}

	private void OnDisable()
	{
		unityDragAndDropHook.UninstallHook();
	}

	private void OnDroppedFiles(List<string> files, POINT point)
	{
		if (!slideShow)
		{
			if (files.Count == 1)
			{
				Present(files[0]);
			}
			else
			{
				Present(files, true);
			}
		}
	}

	private void OnRgbdImageProviderEvent(RgbdImageProvider.RgbdImageProviderEventType rgbdImageProviderEventType)
	{
		switch (rgbdImageProviderEventType)
		{
			case RgbdImageProvider.RgbdImageProviderEventType.StartFadeIn:
				outputScreen.FadeIn(FadeInPeriod);
				break;

			case RgbdImageProvider.RgbdImageProviderEventType.StartFadeOut:
				outputScreen.FadeOut(SlideShowFadeOutPeriod);
				break;

			case RgbdImageProvider.RgbdImageProviderEventType.OneCycleOfSlideShowEnded:
				ToNextExhibit();
				break;
		}
	}

	private void ChangeSourceImageProjection(SourceImageProjection sourceImageProjection)
	{
		equirectangularImage.gameObject.SetActive(false);
		    rectangularImage.gameObject.SetActive(false);

		if (sourceImageProjection != SourceImageProjection.Rectangular)
		{
			rgbdImageProvider = equirectangularImage;

			equirectangularImage.HorizontalRange = sourceImageProjection == SourceImageProjection.EquirectangularHalf ? InsideCamera.Range.Half : InsideCamera.Range.Full;
		}
		else
		{
			rgbdImageProvider = rectangularImage;
		}

		rgbdImageProvider.gameObject.SetActive(true);

		rgbdImageProvider.InitState();

		outputScreen.SetRgbdImageTexture(rgbdImageProvider.GetRgbdImageTexture());
	}

	private void ChangeSourceImageProjection()
	{
		SourceImageProjection sourceImageProjection;
		if (rgbdImageProvider == equirectangularImage)
		{
			sourceImageProjection = equirectangularImage.HorizontalRange == InsideCamera.Range.Half ? SourceImageProjection.EquirectangularFull : SourceImageProjection.Rectangular;
		}
		else
		{
			sourceImageProjection = SourceImageProjection.EquirectangularHalf;
		}

		RgbdImageProvider previousRgbdImageProvider = rgbdImageProvider;

		outputScreen.Darken();

		ChangeSourceImageProjection(sourceImageProjection);

		rgbdImageProvider.SourceMediaType        = previousRgbdImageProvider.SourceMediaType;
		rgbdImageProvider.SourceImageArrangement = previousRgbdImageProvider.SourceImageArrangement;
		rgbdImageProvider.ImageFilePath          = previousRgbdImageProvider.ImageFilePath;
		rgbdImageProvider.RgbImageFilePath       = previousRgbdImageProvider.RgbImageFilePath;
		rgbdImageProvider.DepthImageFilePath     = previousRgbdImageProvider.DepthImageFilePath;
		rgbdImageProvider.VideoFilePath          = previousRgbdImageProvider.VideoFilePath;
		rgbdImageProvider.DepthOrientation       = previousRgbdImageProvider.DepthOrientation;

		UpdateConfig1();
	}

	private void ChangeSourceImageArrangementDirection()
	{
		switch (rgbdImageProvider.SourceImageArrangement)
		{
			case RgbdImageProvider.ImageArrangement.LR: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.TB; break;
			case RgbdImageProvider.ImageArrangement.RL: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.BT; break;
			case RgbdImageProvider.ImageArrangement.TB: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.LR; break;
			case RgbdImageProvider.ImageArrangement.BT: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.RL; break;
		}

		UpdateConfig1();
	}

	private void ChangeSourceImageArrangementOrder()
	{
		switch (rgbdImageProvider.SourceImageArrangement)
		{
			case RgbdImageProvider.ImageArrangement.LR: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.RL; break;
			case RgbdImageProvider.ImageArrangement.RL: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.LR; break;
			case RgbdImageProvider.ImageArrangement.TB: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.BT; break;
			case RgbdImageProvider.ImageArrangement.BT: rgbdImageProvider.SourceImageArrangement = RgbdImageProvider.ImageArrangement.TB; break;

			case RgbdImageProvider.ImageArrangement.Separate:
				string rgbImageFilePath              = rgbdImageProvider.RgbImageFilePath;
				rgbdImageProvider.RgbImageFilePath   = rgbdImageProvider.DepthImageFilePath;
				rgbdImageProvider.DepthImageFilePath = rgbImageFilePath;

				if (presentingExhibit != null)
				{
					presentingExhibit.Remove(rgbImageFilePath);
					presentingExhibit.Insert(1, rgbImageFilePath);
				}
				break;
		}

		UpdateConfig1();
	}

	private void ChangeDepthOrientation()
	{
		rgbdImageProvider.DepthOrientation = rgbdImageProvider.DepthOrientation == RgbdImageProvider.ZAxisOrientation.FrontPositive ?
											 RgbdImageProvider.ZAxisOrientation.FrontNegative : RgbdImageProvider.ZAxisOrientation.FrontPositive;

		UpdateConfig1();
	}

	private bool ParseConfig1(string config, RgbdImageProvider.ImageArrangement imageArrangement)
	{
		if (config.Length < 2)
		{
			return false;
		}

		int length = 0;

		SourceImageProjection sourceImageProjection = SourceImageProjection.Rectangular;
		if (config.IndexOf(EquirectangularHalf, System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			sourceImageProjection = SourceImageProjection.EquirectangularHalf;
			length += EquirectangularHalf.Length;
		}
		else if (config.IndexOf(EquirectangularFull, System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			sourceImageProjection = SourceImageProjection.EquirectangularFull;
			length += EquirectangularFull.Length;
		}
		else if (config.IndexOf(Rectangular, System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			sourceImageProjection = SourceImageProjection.Rectangular;
			length += Rectangular.Length;
		}
		else
		{
			sourceImageProjection = SourceImageProjection.Rectangular;
		}

		if (imageArrangement != RgbdImageProvider.ImageArrangement.Separate)
		{
			if (config.IndexOf(LR, System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				imageArrangement = RgbdImageProvider.ImageArrangement.LR;
				length += LR.Length;
			}
			else if (config.IndexOf(RL, System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				imageArrangement = RgbdImageProvider.ImageArrangement.RL;
				length += RL.Length;
			}
			else if (config.IndexOf(TB, System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				imageArrangement = RgbdImageProvider.ImageArrangement.TB;
				length += TB.Length;
			}
			else if (config.IndexOf(BT, System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				imageArrangement = RgbdImageProvider.ImageArrangement.BT;
				length += BT.Length;
			}
			else
			{
				imageArrangement = RgbdImageProvider.ImageArrangement.LR;
			}
		}

		RgbdImageProvider.ZAxisOrientation zAxisOrientation = RgbdImageProvider.ZAxisOrientation.FrontPositive;
		if (config.IndexOf(FrontNegative, System.StringComparison.OrdinalIgnoreCase) < 0)
		{
			zAxisOrientation = RgbdImageProvider.ZAxisOrientation.FrontPositive;
		}
		else
		{
			zAxisOrientation = RgbdImageProvider.ZAxisOrientation.FrontNegative;
			length += FrontNegative.Length;
		}

		if (length != config.Length)
		{
			return false;
		}

		ChangeSourceImageProjection(sourceImageProjection);

		rgbdImageProvider.SourceImageArrangement = imageArrangement;
		rgbdImageProvider.DepthOrientation       = zAxisOrientation;

		return true;
	}

	private void ParseConfig2(string config)
	{
		float position = OutputScreen.Position_Initial;
		float scale    = OutputScreen.Scale_Initial;
		float depth    = OutputScreen.Depth_Initial;

		int z = config.IndexOf("Z");
		int s = config.IndexOf("S");
		int d = config.IndexOf("D");

		if (z >= 0 && z < s && s < d && d < config.Length - 1)
		{
			float value;
			if (float.TryParse(config.Substring(z + 1, s - z - 1), out value)) { position = value; }
			if (float.TryParse(config.Substring(s + 1, d - s - 1), out value)) { scale    = value; }
			if (float.TryParse(config.Substring(d + 1),            out value)) { depth    = value; }
		}

		outputScreen.Position = position;
		outputScreen.Scale    = scale;
		outputScreen.Depth    = depth;
	}

	private void UpdateConfig1()
	{
		if (presentingExhibit == null)
		{
			return;
		}

		string config = "";

		if (rgbdImageProvider == equirectangularImage)
		{
			config += equirectangularImage.HorizontalRange == InsideCamera.Range.Half ? EquirectangularHalf : EquirectangularFull;
		}
		else
		{
			config += Rectangular;
		}

		switch (rgbdImageProvider.SourceImageArrangement)
		{
			default:                                    config += LR; break;
			case RgbdImageProvider.ImageArrangement.RL: config += RL; break;
			case RgbdImageProvider.ImageArrangement.TB: config += TB; break;
			case RgbdImageProvider.ImageArrangement.BT: config += BT; break;
			case RgbdImageProvider.ImageArrangement.Separate:         break;
		}

		if (rgbdImageProvider.DepthOrientation == RgbdImageProvider.ZAxisOrientation.FrontNegative)
		{
			config += FrontNegative;
		}

		presentingExhibit.Remove(presentingExhibit[presentingExhibit.Count - 2]);
		presentingExhibit.Insert(presentingExhibit.Count - 1, config);
	}

	private void UpdateConfig2()
	{
		if (presentingExhibit == null)
		{
			return;
		}

		string config = "";

		config +=  "Z";
		config += outputScreen.Position.ToString("0.00");

		config += " S";
		config += outputScreen.Scale.ToString("0.00");

		config += " D";
		config += outputScreen.Depth.ToString("0.00");

		presentingExhibit.Remove(presentingExhibit[presentingExhibit.Count - 1]);
		presentingExhibit.Add(config);
	}

	private void ToNextExhibit()
	{
		if (defaults.Exhibits.Count == 0)
		{
			return;
		}

		int index = 0;
		if (presentingExhibit != null)
		{
			index = defaults.Exhibits.IndexOf(presentingExhibit) + 1;
			if (index < 0 || index >= defaults.Exhibits.Count)
			{
				index = 0;
			}
		}

		Present(index);
	}

	private void ToPrevExhibit()
	{
		if (defaults.Exhibits.Count == 0)
		{
			return;
		}

		int index = defaults.Exhibits.Count - 1;
		if (presentingExhibit != null)
		{
			index = defaults.Exhibits.IndexOf(presentingExhibit) - 1;
			if (index < 0 || index >= defaults.Exhibits.Count)
			{
				index = defaults.Exhibits.Count - 1;
			}
		}

		Present(index);
	}

	private void Present(int index)
	{
		if (index < 0 || index >= defaults.Exhibits.Count)
		{
			return;
		}

		List<string> exhibit = defaults.Exhibits[index];
		if (exhibit.Count == 3)
		{
			Present(new List<string>() { exhibit[0] });
		}
		else if (exhibit.Count == 4)
		{
			Present(new List<string>() { exhibit[0], exhibit[1] });
		}
	}

	private void RemovePresentingExhibit(bool presentStartImage = false)
	{
		if (presentingExhibit == null || defaults.Exhibits.Count == 0)
		{
			return;
		}

		int index = defaults.Exhibits.IndexOf(presentingExhibit) - 1;
		if (index < 0 || index >= defaults.Exhibits.Count)
		{
			index = defaults.Exhibits.Count - 1;
		}

		List<string> exhibit = defaults.Exhibits[index];

		defaults.Exhibits.Remove(presentingExhibit);

		if (!presentStartImage)
		{
			index = defaults.Exhibits.IndexOf(exhibit);
			if (index >= 0)
			{
				Present(index);
				return;
			}
		}

		Present(new List<string>() { startImageFilePath });
	}

	private string[] imageFileExtensions =
	{
		".bmp", ".gif", ".iff", ".jpg", ".pict", ".png", ".psd", ".tga", ".tif", ".tiff"
	};

	private bool IsImageFile(string filePath)
	{
		string extension = Path.GetExtension(filePath);

		foreach (string imageFileExtension in imageFileExtensions)
		{
			if (string.Compare(extension, imageFileExtension, true) == 0)
			{
				return true;
			}
		}

		return false;
	}

	private string[] videoFileExtensions =
	{
		".asf", ".avi", ".mov", ".mp4", ".mpeg", ".mpg"
	};

	private bool IsVideoFile(string filePath)
	{
		string extension = Path.GetExtension(filePath);

		foreach (string videoFileExtension in videoFileExtensions)
		{
			if (string.Compare(extension, videoFileExtension, true) == 0)
			{
				return true;
			}
		}

		return false;
	}

	private string GetFileNameSuffix(string filePath)
	{
		string name = Path.GetFileNameWithoutExtension(filePath);

		int underbar = name.LastIndexOf("_", System.StringComparison.OrdinalIgnoreCase);

		return underbar >= 0 && underbar < name.Length - 1 ? name.Substring(underbar + 1) : "";
	}

	private void Present(string filePath)
	{
		if (!IsImageFile(filePath))
		{
			Present(new List<string>() { filePath }, true);
			return;
		}

		string directoryName = Path.GetDirectoryName(filePath);
		string      fileName = Path.GetFileNameWithoutExtension(filePath);

		string prefix = "depth_";
		string suffix = "_depth";

		List<string> names = null;

		int index = fileName.IndexOf(prefix, System.StringComparison.OrdinalIgnoreCase);
		if (index >= 0)
		{
			names = new List<string>() { fileName.Remove(index, prefix.Length) };
		}

		if (names == null)
		{
			index = fileName.IndexOf(suffix, System.StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				names = new List<string>() { fileName.Remove(index, suffix.Length) };
			}
		}

		if (names == null)
		{
			names = new List<string>() { prefix + fileName, fileName + suffix };
		}

		foreach (string name in names)
		{
			foreach (string extension in imageFileExtensions)
			{
				string pair = directoryName + "\\" + name + extension;

				if (File.Exists(pair))
				{
					Present(new List<string>() { filePath, pair }, true);
					return;
				}
			}
		}

		Present(new List<string>() { filePath }, true);
	}

	private void Present(List<string> files, bool droppedFile = false)
	{
		RgbdImageProvider.MediaType mediaType = RgbdImageProvider.MediaType.Image;
		RgbdImageProvider.ImageArrangement imageArrangement = rgbdImageProvider != null ?
															  rgbdImageProvider.SourceImageArrangement : RgbdImageProvider.ImageArrangement.LR;
		string imageFilePath      = "";
		string rgbImageFilePath   = "";
		string depthImageFilePath = "";
		string videoFilePath      = "";
		string exhibitFilePath    = "";
		string config1            = "";
		string config2            = "";

		if (files.Count == 1)
		{
			if (IsImageFile(files[0]))
			{
				mediaType = RgbdImageProvider.MediaType.Image;

				imageFilePath   = files[0];
				exhibitFilePath = imageFilePath;
			}
			else if (IsVideoFile(files[0]))
			{
				mediaType = RgbdImageProvider.MediaType.Video;

				videoFilePath   = files[0];
				exhibitFilePath = videoFilePath;
			}

			if (imageArrangement == RgbdImageProvider.ImageArrangement.Separate)
			{
				imageArrangement =  RgbdImageProvider.ImageArrangement.LR;
			}

			config1 = GetFileNameSuffix(files[0]);
		}
		else if (files.Count == 2)
		{
			if (IsImageFile(files[0]) && IsImageFile(files[1]))
			{
				for (int i = 0; i < 2; i++)
				{
					if (Path.GetFileNameWithoutExtension(files[i]).IndexOf("depth", System.StringComparison.OrdinalIgnoreCase) >= 0)
					{
						  rgbImageFilePath = files[(i + 1) % 2];
						depthImageFilePath = files[i];
						   exhibitFilePath = rgbImageFilePath;

						break;
					}
				}

				if (exhibitFilePath.Length == 0)
				{
					  rgbImageFilePath = files[0];
					depthImageFilePath = files[1];
					   exhibitFilePath = rgbImageFilePath;
				}

				mediaType        = RgbdImageProvider.MediaType.Image;
				imageArrangement = RgbdImageProvider.ImageArrangement.Separate;

				config1 = GetFileNameSuffix(rgbImageFilePath);
			}
		}

		if (exhibitFilePath.Length == 0)
		{
			Present(new List<string>() { startImageFilePath });
			return;
		}

		presentingExhibit = null;
		   droppedExhibit = droppedFile;

		if (exhibitFilePath != startImageFilePath)
		{
			presentingExhibit = defaults.Exhibits.Find(exhibit => exhibit[0] == exhibitFilePath);
			if (presentingExhibit == null || presentingExhibit.Count < 3)
			{
				presentingExhibit = new List<string> { exhibitFilePath, "", "" };

				if (depthImageFilePath.Length > 0)
				{
					presentingExhibit.Insert(1, depthImageFilePath);
				}

				defaults.Exhibits.Add(presentingExhibit);
			}
			else
			{
				config1 = presentingExhibit[presentingExhibit.Count - 2];
				config2 = presentingExhibit[presentingExhibit.Count - 1];

				if (droppedFile)
				{
					defaults.Exhibits.Remove(presentingExhibit);
					defaults.Exhibits.Add(   presentingExhibit);
				}
			}
		}

		if (rgbdImageProvider != null)
		{
			rgbdImageProvider.InitState();
		}

		outputScreen.Darken();

		if (!ParseConfig1(config1, imageArrangement))
		{
			rgbdImageProvider.SourceImageArrangement = imageArrangement;

			// for Kandao QooCam
			if (imageArrangement == RgbdImageProvider.ImageArrangement.Separate)
			{
				if (Path.GetFileNameWithoutExtension(rgbImageFilePath).IndexOf("Output_KD3D", System.StringComparison.OrdinalIgnoreCase) >= 0)
				{
					ChangeSourceImageProjection(SourceImageProjection.EquirectangularHalf);

					rgbdImageProvider.SourceImageArrangement = imageArrangement;
					rgbdImageProvider.DepthOrientation       = RgbdImageProvider.ZAxisOrientation.FrontPositive;
				}
			}
		}

		ParseConfig2(config2);

		rgbdImageProvider.SourceMediaType    = mediaType;
		rgbdImageProvider.ImageFilePath      = imageFilePath;
		rgbdImageProvider.RgbImageFilePath   = rgbImageFilePath;
		rgbdImageProvider.DepthImageFilePath = depthImageFilePath;
		rgbdImageProvider.VideoFilePath      = videoFilePath;

		UpdateConfig1();
		UpdateConfig2();

		if (slideShow)
		{
			rgbdImageProvider.StartOneCycleOfSlideShow(SlideShowOneCyclePeriod, SlideShowRestPeriod, SlideShowFadeOutPeriod);
		}
	}
}
