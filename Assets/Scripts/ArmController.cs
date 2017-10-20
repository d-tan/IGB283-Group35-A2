using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Joints {
	Base,
	Lower,
	Upper,
	Wrist
}

public enum SoundEffects {
	Jump,
	Fall,
	GetUp,
	Wall
}

public class ArmController : MonoBehaviour {

	public Limb[] parts;
	public AudioClip[] sounds;
	AudioSource audioSource;

	// Moving
	float moveSpeed = 5f; // normal move speed
	float currentMoveSpeed = 5f; // How fast the arm moves left or right currently
	int moveDirection = -1;
	Vector2 moveBounds = new Vector2 (-10, 10); // The edges that the arm can't move past (X-Axis)

	// Nodding
	float nodSpeed = 100f; // How fast the wrist rotates (Euler angles/second)
	Vector2 nodAngles = new Vector2 (-90, 0);
	int nodDirection = 1;
	int rotDirection = -1;
	bool isNodding = true;

	// Jumping
	float jumpSpeed = 5f; // How fast the arm goes up
	float jumpForward = 0; // For Jumping forward
	Vector2 jumpBounds = new Vector2(0, 3);
	float gravity = 9.8f;
	int jumpDirection = 1;

	// Falling
	bool isFalling = false;
	Vector4 fallPos = new Vector4(90, 0, 0, 0);

	// Getting Up
	bool isGettingUp = false;
	float getupTimer = 0f;
	float getupTime = 3f;
	Vector4 upPos = new Vector4(0, 0, 0, 0);

	void Start() {
		audioSource = GetComponent<AudioSource> ();
	}

	void Update() {
		// Keep nodding wrist
		if (isNodding)
			NodWrist ();

		// Holding 'A' key, move left
		if (!isFalling && Input.GetKey(KeyCode.A)) {

			// Check if still in bounds
			if (parts [(int)Joints.Base].jointLocation.x > moveBounds.x) {
				moveDirection = -1;
				currentMoveSpeed = moveSpeed;
			} else { 
				currentMoveSpeed = 0;
				jumpForward = 0;
			}
		
		// Holding 'D' key, move right
		} else if (!isFalling && Input.GetKey(KeyCode.D)) {

			// Check if still in bounds
			if (parts [(int)Joints.Base].jointLocation.x < moveBounds.y) {
				moveDirection = 1;
				currentMoveSpeed = moveSpeed;
			} else {
				currentMoveSpeed = 0;
				jumpForward = 0;
			}
		}
		// Move
		MoveLeftRight ();


		// Keep Jumping
		if (jumpDirection == 0) {
			if (!isFalling && Input.GetKey (KeyCode.W)) {
				jumpDirection = 1;
				PlaySound (SoundEffects.Jump);
			} else if (!isFalling && Input.GetKey (KeyCode.S)) {
				jumpDirection = 1;
				jumpForward = moveSpeed;
				PlaySound (SoundEffects.Jump);
			}
		}
		Jump ();

		// Stop all movement and set the jump direction to down (i.e. base will fall)
		if (!isFalling && Input.GetKeyDown(KeyCode.Z)) {
			jumpForward = 0;
			currentMoveSpeed = 0;
			jumpDirection = -1;
			isFalling = true;
			isNodding = false;
			RotationChange (fallPos);
		}

		// Get up animation
		if (isGettingUp) {
			getupTimer += Time.deltaTime;

			if (getupTimer >= getupTime) {
				getupTimer = 0;
				isGettingUp = false;
				isNodding = true;
				RotationChange (upPos);
				currentMoveSpeed = moveSpeed;
				PlaySound (SoundEffects.GetUp);
			}
		}
	}

	void Jump() {
		// Move base up or down
		parts [(int)Joints.Base].MoveByOffset(new Vector3(0, jumpDirection, 0) * (jumpDirection > 0 ? jumpSpeed : gravity) * Time.deltaTime);

		// Check if base has reach the boundary
		if (jumpDirection > 0 && parts [(int)Joints.Base].jointLocation.y >= jumpBounds.y) {
			jumpDirection = -1;
		} else if (jumpDirection < 0 && parts [(int)Joints.Base].jointLocation.y <= jumpBounds.x) {
			// Stop movement
			jumpDirection = 0;
			if (jumpForward > 0)
				currentMoveSpeed = 0;
			jumpForward = 0;

			if (isFalling) {
				isFalling = false;
				isGettingUp = true;
				PlaySound (SoundEffects.Fall);
			}

			// reset the y position to 0
			parts [(int)Joints.Base].MoveByOffset (new Vector3 (0, -parts [(int)Joints.Base].jointLocation.y, 0));
		}
	}

	void MoveLeftRight() {
		// Move the base joint
		parts [(int)Joints.Base].MoveByOffset (new Vector3 (moveDirection, 0, 0) * (currentMoveSpeed + jumpForward) * Time.deltaTime);

		// Check if the player wants to move
		if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
			// Check if base is about to leave the boundary
			if (moveDirection < 0 && parts [(int)Joints.Base].jointLocation.x <= moveBounds.x) {
				moveDirection = 1;
			PlaySound (SoundEffects.Wall);
			} else if (moveDirection > 0 && parts [(int)Joints.Base].jointLocation.x >= moveBounds.y) {
				moveDirection = -1;
			PlaySound (SoundEffects.Wall);
			}
	}

	void NodWrist() {
		// Store angle
		float angle = parts [(int)Joints.Upper].angle;

		// Store current angle as previous
		parts [(int)Joints.Upper].lastAngle = angle;

		// Add rotation
		angle += rotDirection * nodSpeed * Mathf.Deg2Rad * Time.deltaTime;

		// Check if angle is passed what we want
		if (rotDirection < 0 && angle <= nodAngles.x * Mathf.Deg2Rad) {
			angle = nodAngles.x * Mathf.Deg2Rad;
			rotDirection = 1;

		} else if (rotDirection > 0 && angle >= nodAngles.y * Mathf.Deg2Rad) {
			angle = nodAngles.y * Mathf.Deg2Rad;
			rotDirection = -1;
		}

		// Set the new angle to rotate to
		parts [(int)Joints.Upper].angle = angle;
	}

	void RotationChange(Vector4 angles) {
		// Rotate Base
		parts [(int)Joints.Base].angleChanged = true;
		parts [(int)Joints.Base].lastAngle = parts [(int)Joints.Base].angle;
		parts [(int)Joints.Base].angle = angles.x * Mathf.Deg2Rad;

		// Rotate Upper
		parts [(int)Joints.Upper].angleChanged = true;
		parts [(int)Joints.Upper].lastAngle = parts [(int)Joints.Upper].angle;
		parts [(int)Joints.Upper].angle = angles.y * Mathf.Deg2Rad;

		// Rotate Lower
		parts [(int)Joints.Lower].angleChanged = true;
		parts [(int)Joints.Lower].lastAngle = parts [(int)Joints.Lower].angle;
		parts [(int)Joints.Lower].angle = angles.z * Mathf.Deg2Rad;

		// Rotate Wrist
		parts [(int)Joints.Wrist].angleChanged = true;
		parts [(int)Joints.Wrist].lastAngle = parts [(int)Joints.Wrist].angle;
		parts [(int)Joints.Wrist].angle = angles.w * Mathf.Deg2Rad;
	}

	void PlaySound(SoundEffects sound) {
		audioSource.clip = sounds [(int)sound];
		audioSource.pitch = Random.Range (0.9f, 1.1f);
		audioSource.Play ();
	}
}
