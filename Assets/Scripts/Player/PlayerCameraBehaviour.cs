﻿using UnityEngine;

public class PlayerCameraBehaviour : MonoBehaviour
{
	[SerializeField] private Transform playerBody;
	[Space]
	[SerializeField] private string mouseXInputName = default;
	[SerializeField] private string mouseYInputName = default;
	[SerializeField] private float mouseSensitivity = default;


	private float mouseXAxisClamp = 90f;

	private float xAxisClamp = default;

	private void Awake()
	{
		LockCursor(true);
		xAxisClamp = 0.0f;
	}

	private void Update()
	{
		RotateCamera();
	}

	/// <summary>
	/// Rotates camera relative to the mouse movement.
	/// </summary>
	private void RotateCamera()
	{
		float multiplier = mouseSensitivity;
		float mouseX = Input.GetAxisRaw(mouseXInputName) * multiplier;
		float mouseY = Input.GetAxisRaw(mouseYInputName) * multiplier;

		xAxisClamp += mouseY;

		if(xAxisClamp > mouseXAxisClamp)
		{
			xAxisClamp = 90f;
			mouseY = 0f;
			ClampXAxisRotationToValue(270);
		}
		else if(xAxisClamp < -mouseXAxisClamp)
		{
			xAxisClamp = -90f;
			mouseY = 0f;
			ClampXAxisRotationToValue(90);
		}

		transform.Rotate(Vector3.left * mouseY);
		playerBody.Rotate(Vector3.up * mouseX);
	}

	/// <summary>
	/// Clamps rotation so the player cant do loops with the camera.
	/// </summary>
	/// <param name="value"></param>
	private void ClampXAxisRotationToValue(float value)
	{
		Vector3 eulerRotation = transform.eulerAngles;
		eulerRotation.x = value;
		transform.eulerAngles = eulerRotation;
	}

	/// <summary>
	/// (un)locks the cursor.
	/// </summary>
	/// <param name="lockState"></param>
	private void LockCursor(bool lockState)
	{
		Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = lockState ? false : true;
	}
}
