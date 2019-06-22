using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquirectangularImage : RgbdImageProvider
{
	public InsideCamera insideCamera;

	public InsideCamera.Range HorizontalRange
	{
		set { insideCamera.HorizontalRange = value; }
		get { return insideCamera.HorizontalRange;  }
	}

	private RenderTexture rgbdImageTexture = null;

	private Material material = null;

	protected override void OnStart()
	{
		rgbdImageTexture = new RenderTexture(rgbdImageTextureWidth, rgbdImageTextureHeight, 24);

		insideCamera.gameObject.GetComponent<Camera>().targetTexture = rgbdImageTexture;

		material = GameObject.Find("/EquirectangularImage/SphericalScreen").GetComponent<Renderer>().material;
	}

	protected override void OnUpdate()
	{
	}

	public override void OnDrag(Vector3 delta)
	{
		insideCamera.OnDrag(delta);
	}

	public override void OnWheel(float delta)
	{
		insideCamera.OnWheel(delta);
	}

	protected override void ResetCamera()
	{
		insideCamera.ResetCamera();
	}

	protected override void PrepareCameraMoving(CameraPosition from, CameraPosition to)
	{
		insideCamera.PrepareCameraMoving(from, to);
	}

	protected override void MoveCamera(float ratio)
	{
		insideCamera.MoveCamera(ratio);
	}

	public override Texture GetRgbdImageTexture()
	{
		return rgbdImageTexture;
	}

	protected override Material GetMaterial()
	{
		return material;
	}

	protected override string GetSuitableShaderName(ImageArrangement imageArrangement)
	{
		if (insideCamera.HorizontalRange == InsideCamera.Range.Half)
		{
			switch (imageArrangement)
			{
				default:                        return "EquirectangularImage/SphericalScreen/HalfLR";
				case ImageArrangement.RL:       return "EquirectangularImage/SphericalScreen/HalfRL";
				case ImageArrangement.TB:       return "EquirectangularImage/SphericalScreen/HalfTB";
				case ImageArrangement.BT:       return "EquirectangularImage/SphericalScreen/HalfBT";
				case ImageArrangement.Separate: return "EquirectangularImage/SphericalScreen/HalfSeparate";
			}
		}
		else
		{
			switch (imageArrangement)
			{
				default:                        return "EquirectangularImage/SphericalScreen/FullLR";
				case ImageArrangement.RL:       return "EquirectangularImage/SphericalScreen/FullRL";
				case ImageArrangement.TB:       return "EquirectangularImage/SphericalScreen/FullTB";
				case ImageArrangement.BT:       return "EquirectangularImage/SphericalScreen/FullBT";
				case ImageArrangement.Separate: return "EquirectangularImage/SphericalScreen/FullSeparate";
			}
		}
	}
}
