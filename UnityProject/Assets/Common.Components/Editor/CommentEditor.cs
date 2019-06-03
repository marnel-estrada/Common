using System.Collections;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Comment))]
public class CommentEditor : Editor {

	private Comment targetComponent;

	void OnEnable() {
		this.targetComponent = (Comment)this.target;
	}

	public override void OnInspectorGUI() {
		if(targetComponent.Text == null) {
			targetComponent.Text = ""; // use an empty string so that it won't throw null pointer exception
		}

		targetComponent.Text = GUILayout.TextArea(targetComponent.Text, GUILayout.Height(100), GUILayout.MinWidth(200));
	}

}
