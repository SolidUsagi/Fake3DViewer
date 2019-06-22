using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideCamera : MonoBehaviour
{
	public Material material;

	public enum Range
	{
		Half,
		Full
	}

	private Range horizontalRange = Range.Half;
	public  Range HorizontalRange
	{
		set { this.horizontalRange = value; }
		get { return this.horizontalRange;  }
	}

	private const float FieldOfView_Initial = 60.0f;
	private const float FieldOfView_Lower   = 30.0f;
	private const float FieldOfView_Upper   = 90.0f;

	private Quaternion rotation_From    = Quaternion.identity;
	private Quaternion rotation_To      = Quaternion.identity;
	private float      fieldOfView_From = FieldOfView_Initial;
	private float      fieldOfView_To   = FieldOfView_Initial;

	private float mouseMovingRatio = 1.0f;

	void Start()
	{
		GetComponent<Camera>().fieldOfView = FieldOfView_Initial;

		mouseMovingRatio = 360.0f / Screen.currentResolution.height;
	}

	void Update()
	{
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}

	public void OnDrag(Vector3 delta)
	{
		Vector3 eulerAngles = transform.rotation.eulerAngles;
		eulerAngles.x = eulerAngles.x < 180.0f ? eulerAngles.x : eulerAngles.x - 360.0f;
		eulerAngles.y = eulerAngles.y < 180.0f ? eulerAngles.y : eulerAngles.y - 360.0f;

		float yaw;
		float pitch;
		if (horizontalRange == Range.Half)
		{
			yaw   = -delta.x * mouseMovingRatio; yaw   = yaw   >= 0.0f ? System.Math.Min(yaw,   90.0f - eulerAngles.y) : System.Math.Max(yaw,   -90.0f - eulerAngles.y);
			pitch =  delta.y * mouseMovingRatio; pitch = pitch >= 0.0f ? System.Math.Min(pitch, 90.0f - eulerAngles.x) : System.Math.Max(pitch, -90.0f - eulerAngles.x);
		}
		else
		{
			yaw   = -delta.x * mouseMovingRatio;
			pitch =  delta.y * mouseMovingRatio; pitch = pitch >= 0.0f ? System.Math.Min(pitch, 90.0f - eulerAngles.x) : System.Math.Max(pitch, -90.0f - eulerAngles.x);
		}

		Quaternion rotation = Quaternion.AngleAxis(yaw,     Vector3.up   )
							* Quaternion.AngleAxis(pitch, transform.right)
							* transform.rotation;

		transform.rotation = System.Math.Abs(rotation.eulerAngles.z) < 1.0f ? rotation : transform.rotation;
	}

	public void OnWheel(float delta)
	{
		GetComponent<Camera>().fieldOfView = System.Math.Max(FieldOfView_Lower, System.Math.Min(GetComponent<Camera>().fieldOfView - delta * 10, FieldOfView_Upper));
	}

	public void ResetCamera()
	{
		transform.rotation = Quaternion.identity;
		GetComponent<Camera>().fieldOfView = FieldOfView_Initial;
	}

	public void PrepareCameraMoving(RgbdImageProvider.CameraPosition from, RgbdImageProvider.CameraPosition to)
	{
		switch (from)
		{
			case RgbdImageProvider.CameraPosition.Initial:
				rotation_From    = Quaternion.identity;
				fieldOfView_From = FieldOfView_Initial;
				break;

			case RgbdImageProvider.CameraPosition.Current:
				rotation_From    = transform.rotation;
				fieldOfView_From = GetComponent<Camera>().fieldOfView;
				break;

			case RgbdImageProvider.CameraPosition.Random:
				rotation_From    = GetRandomRotation();
				fieldOfView_From = GetRandomFieldOfView();
				break;
		}

		switch (to)
		{
			case RgbdImageProvider.CameraPosition.Initial:
				rotation_To    = Quaternion.identity;
				fieldOfView_To = FieldOfView_Initial;
				break;

			case RgbdImageProvider.CameraPosition.Current:
				rotation_To    = transform.rotation;
				fieldOfView_To = GetComponent<Camera>().fieldOfView;
				break;

			case RgbdImageProvider.CameraPosition.Random:
				rotation_To    = GetRandomRotation();
				fieldOfView_To = GetRandomFieldOfView();
				break;
		}
	}

	public void MoveCamera(float ratio)
	{
		transform.rotation = Quaternion.Slerp(rotation_From, rotation_To, ratio);
		GetComponent<Camera>().fieldOfView = Mathf.Lerp(fieldOfView_From, fieldOfView_To, ratio);
	}

	private Quaternion GetRandomRotation()
	{
		float yaw   = horizontalRange == Range.Half ? Random.Range(-45.0f, 45.0f) : Random.Range(-180.0f, 180.0f);
		float pitch = Random.Range(-20.0f, 20.0f);

		return Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
	}

	private float GetRandomFieldOfView()
	{
		return Random.Range(FieldOfView_Lower, FieldOfView_Upper);
	}
}
