using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Limb : MonoBehaviour {

	public GameObject child;
	public GameObject control;

	public Vector3 jointLocation;
	public Vector3 jointOffset;

	public float angle;

	public float lastAngle;

	[HideInInspector]
	public bool angleChanged = false;

	public Vector3[] limbVertexLocations;

	public Mesh mesh;
	public Material material;

	void Awake() {
		DrawLimb ();
	}

	void Start() {
//		DrawLimb ();

		if (child != null) {
			child.GetComponent<Limb> ().MoveByOffset (jointOffset);
		}
	}

	void Update() {
//		lastAngle = angle;

		if (control != null) {
//			angle = control.GetComponent<Slider> ().value;
		}

		if (child != null) {
			child.GetComponent<Limb> ().RotateAroundPoint (jointLocation, angle, lastAngle);
		}

		mesh.RecalculateBounds ();

		if (angleChanged) {
			angleChanged = false;
			lastAngle = angle;
		}
	}

	void DrawLimb() {
		gameObject.AddComponent<MeshFilter> ();
		gameObject.AddComponent<MeshRenderer> ();

		mesh = GetComponent<MeshFilter> ().mesh;
		GetComponent<MeshRenderer> ().material = material;

		if (limbVertexLocations.Length >= 4) {
			mesh.vertices = limbVertexLocations;

			mesh.triangles = new int[]{ 0, 1, 2, 2, 3, 0};
		}
	}

	public void MoveByOffset(Vector3 offset) {
		Matrix3x3 T = Matrix3x3.TranslationMatrix (offset);
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] = T.MultiplyPoint (vertices [i]);
		}

		mesh.vertices = vertices;

		jointLocation = T.MultiplyPoint (jointLocation);

		if (child != null) {
			child.GetComponent<Limb> ().MoveByOffset (offset);
		}
	}

	public void RotateAroundPoint(Vector3 point, float angle, float lastAngle) {
		Matrix3x3 T1 = Matrix3x3.TranslationMatrix (-point);

		Matrix3x3 R1 = Matrix3x3.RotationMatrix(-lastAngle);

		Matrix3x3 T2 = Matrix3x3.TranslationMatrix (point);

		Matrix3x3 R2 = Matrix3x3.RotationMatrix (angle);

		Matrix3x3 M = T2 * R2 * R1 * T1;

		Vector3[] vertices = mesh.vertices;

		for(int i = 0; i < vertices.Length; i++) {
			vertices [i] = M.MultiplyPoint (vertices [i]);
		}

		mesh.vertices = vertices;

		jointLocation = M.MultiplyPoint (jointLocation);

		if (child != null) {
			child.GetComponent<Limb> ().RotateAroundPoint(point, angle, lastAngle);
		}
	}
}
