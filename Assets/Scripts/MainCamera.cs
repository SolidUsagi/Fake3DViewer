using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
	private bool    move      = false;
	private bool    plus      = true;
	private float   angle     =  0.0f;
	private float   speed     = 50.0f;
	private float   distance  =  1.0f;
	private Vector3 viewpoint = new Vector3(0.0f, 0.0f, 0.0f);

	void Start()
	{
		distance = -transform.position.z;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			move = !move;
		}

		if (!move)
		{
			return;
		}

		if (plus)
		{
			angle += Time.deltaTime * speed;
			if (angle > 0.0f + 45.0f / 2)
			{
				angle = 0.0f + 45.0f / 2;
				plus  = false;
			}
		}
		else
		{
			angle -= Time.deltaTime * speed;
			if (angle < 0.0f - 45.0f / 2)
			{
				angle = 0.0f - 45.0f / 2;
				plus  = true;
			}
		}

		transform.localPosition = viewpoint;
		transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
		transform.Translate(new Vector3(0.0f, 0.0f, -distance));
	}
}
