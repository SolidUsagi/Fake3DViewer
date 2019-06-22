using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangularImage : RgbdImageProvider
{
	private CustomRenderTexture rgbdImageTexture = null;

	private bool changeView = false;

	private static Vector2 Position_Initial = new Vector2(0.5f, 0.5f);
	private Vector2 position = Position_Initial;

	private const float Scale_Initial = 1.0f;
	private const float Scale_Lower   = 1.0f;
	private const float Scale_Upper   = 2.0f;
	private float scale = Scale_Initial;

	private Vector2 position_From = Position_Initial;
	private Vector2 position_To   = Position_Initial;
	private float   scale_From    = Scale_Initial;
	private float   scale_To      = Scale_Initial;

	private float mouseMovingRatio = 1.0f;

	private Material material = null;

	protected override void OnStart()
	{
		rgbdImageTexture = new CustomRenderTexture(rgbdImageTextureWidth, rgbdImageTextureHeight);
		rgbdImageTexture.material   = new Material(Shader.Find("RectangularImage/LR"));
		rgbdImageTexture.updateMode = CustomRenderTextureUpdateMode.OnDemand;

		mouseMovingRatio = 2.0f / Screen.currentResolution.height;

		// Unityエディター起動後初めてプレイモードに入ったとき、OnStartメソッド内で rgbdImageTexture.material にインスタンスを格納しているのに
		// OnStartメソッドを抜けた後に rgbdImageTexture.material == null になってしまうという現象が発生する。
		// 以下のように別のフィールドでMaterialインスタンスを保持しておくと現象は発生しないので、GC周りの問題か？
		material = rgbdImageTexture.material;
	}

	protected override void OnUpdate()
	{
		if (changeView)
		{
			int imageWidth  = ImageWidth;
			int imageHeight = ImageHeight;

			float uScale = 1.0f;
			float vScale = 1.0f;
			if (imageWidth > imageHeight)
			{
				vScale = (float)imageWidth / imageHeight;
			}
			else if (imageWidth < imageHeight)
			{
				uScale = (float)imageHeight / imageWidth;
			}

			Material material = GetMaterial();

			if (material.HasProperty("_U"))
			{
				material.SetFloat("_U", position.x);
			}

			if (material.HasProperty("_V"))
			{
				material.SetFloat("_V", position.y);
			}

			if (material.HasProperty("_UScale"))
			{
				material.SetFloat("_UScale", uScale / scale);
			}

			if (material.HasProperty("_VScale"))
			{
				material.SetFloat("_VScale", vScale / scale);
			}
		}

		changeView = false;

		rgbdImageTexture.Update();
	}

	protected override void OnSetTextureToShader()
	{
		changeView = true;
	}

	protected override void OnVideoPlayerPrepareCompleted()
	{
		changeView = true;
	}

	public override void OnDrag(Vector3 delta)
	{
		position.x = System.Math.Max(0.0f, System.Math.Min(position.x - delta.x * mouseMovingRatio, 1.0f));
		position.y = System.Math.Max(0.0f, System.Math.Min(position.y - delta.y * mouseMovingRatio, 1.0f));

		changeView = true;
	}

	public override void OnWheel(float delta)
	{
		scale = System.Math.Max(Scale_Lower, System.Math.Min(scale + delta * 0.2f, Scale_Upper));

		changeView = true;
	}

	protected override void ResetCamera()
	{
		position = Position_Initial;
		scale    = Scale_Initial;
	}

	protected override void PrepareCameraMoving(CameraPosition from, CameraPosition to)
	{
		switch (from)
		{
			case RgbdImageProvider.CameraPosition.Initial:
				position_From = Position_Initial;
				scale_From    = Scale_Initial;
				break;

			case RgbdImageProvider.CameraPosition.Current:
				position_From = position;
				scale_From    = scale;
				break;

			case RgbdImageProvider.CameraPosition.Random:
				position_From = GetRandomPosition();
				scale_From    = GetRandomScale();
				break;
		}

		switch (to)
		{
			case RgbdImageProvider.CameraPosition.Initial:
				position_To = Position_Initial;
				scale_To    = Scale_Initial;
				break;

			case RgbdImageProvider.CameraPosition.Current:
				position_To = position;
				scale_To    = scale;
				break;

			case RgbdImageProvider.CameraPosition.Random:
				position_To = GetRandomPosition();
				scale_To    = GetRandomScale();
				break;
		}
	}

	protected override void MoveCamera(float ratio)
	{
		position = Vector2.Lerp(position_From, position_To, ratio);
		scale    =   Mathf.Lerp(scale_From,    scale_To,    ratio);

		changeView = true;
	}

	private Vector2 GetRandomPosition()
	{
		return new Vector2(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f));
	}

	private float GetRandomScale()
	{
		return Random.Range(Scale_Lower, Scale_Upper);
	}

	public override Texture GetRgbdImageTexture()
	{
		return rgbdImageTexture;
	}

	protected override Material GetMaterial()
	{
		return rgbdImageTexture.material;
	}

	protected override string GetSuitableShaderName(ImageArrangement imageArrangement)
	{
		switch (imageArrangement)
		{
			default:                        return "RectangularImage/LR";
			case ImageArrangement.RL:       return "RectangularImage/RL";
			case ImageArrangement.TB:       return "RectangularImage/TB";
			case ImageArrangement.BT:       return "RectangularImage/BT";
			case ImageArrangement.Separate: return "RectangularImage/Separate";
		}
	}
}
