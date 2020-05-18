using Common;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RotationInRadians))]
public class RotationInRadiansEditor : Editor {

	private Transform transform;

	void OnEnable() {
		this.transform = ((RotationInRadians)this.target).transform;
		Assertion.NotNull(this.transform);
	}
	
	public override void OnInspectorGUI() {
		GUILayout.BeginHorizontal();
		
		GUILayout.Label("X: ", GUILayout.Width(10));
		GUILayout.Label(ToRadians(transform.eulerAngles.x).ToString());
		
		GUILayout.Label("Y: ", GUILayout.Width(10));
		GUILayout.Label(ToRadians(transform.eulerAngles.y).ToString());
		
		GUILayout.Label("Z: ", GUILayout.Width(10));
		GUILayout.Label(ToRadians(transform.eulerAngles.z).ToString());
		
		GUILayout.EndHorizontal();
	}
	
	private static float ToRadians(float angle) {
		return Mathf.Deg2Rad * angle;
	}

}
