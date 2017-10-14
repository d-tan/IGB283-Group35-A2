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

	float nodSpeed = 100f;
	Vector2 nodAngles = new Vector2 (-93, 5);
	int nodDirection = 1;
	int rotDirection = -1;

	void Update() {
		NodWrist ();
	}


	void NodWrist() {

		float angle = parts [(int)Joints.Upper].angle;
		parts [(int)Joints.Upper].lastAngle = angle;

		angle += rotDirection * nodSpeed * Mathf.Deg2Rad * Time.deltaTime;

		if (rotDirection < 0 && angle <= nodAngles.x * Mathf.Deg2Rad) {
			angle = nodAngles.x * Mathf.Deg2Rad;
			rotDirection = 1;
		} else if (rotDirection > 0 && angle >= nodAngles.y * Mathf.Deg2Rad) {
			angle = nodAngles.y * Mathf.Deg2Rad;
			rotDirection = -1;
		}

//		Debug.Log (angle);
		parts [(int)Joints.Upper].angle = angle;
	}
}
