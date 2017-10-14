using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Joints {
	Base,
	Lower,
	Upper,
	Wrist
}

public class ArmController : MonoBehaviour {

	public Limb[] parts;

	// Moving
	float moveSpeed = 5f;
	float currentMoveSpeed = 5f;
	int moveDirection = -1;
	Vector2 moveBounds = new Vector2 (-10, 10);

	// Nodding
	float nodSpeed = 100f;
	Vector2 nodAngles = new Vector2 (-90, 0);
	int nodDirection = 1;
	int rotDirection = -1;

	// Jumping
	float jumpSpeed = 5f;
	float jumpForward = 0;
	Vector2 jumpBounds = new Vector2(0, 3);
	float gravity = 9.8f;
	int jumpDirection = 1;

	void Update() {
		// Keep nodding wrist
		NodWrist ();

		// Holding 'A' key, move left
		if (Input.GetKey(KeyCode.A)) {

			// Check if still in bounds
			if (parts [(int)Joints.Base].jointLocation.x > moveBounds.x) {
				moveDirection = -1;
				currentMoveSpeed = moveSpeed;
			} else { 
				currentMoveSpeed = 0;
				jumpForward = 0;
			}
		
		// Holding 'D' key, move right
		} else if (Input.GetKey(KeyCode.D)) {

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
		if (jumpDirection == 0)
			if (Input.GetKey(KeyCode.W)) {
				jumpDirection = 1;
			} else if (Input.GetKey(KeyCode.S)) {
				jumpDirection = 1;
				jumpForward = moveSpeed;
			}
		Jump ();

		// Stop all movement and set the jump direction to down (i.e. base will fall)
		if (Input.GetKeyDown(KeyCode.Z)) {
			jumpForward = 0;
			currentMoveSpeed = 0;
			jumpDirection = -1;
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
			} else if (moveDirection > 0 && parts [(int)Joints.Base].jointLocation.x >= moveBounds.y) {
				moveDirection = -1;
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
}
