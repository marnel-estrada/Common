using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(Comment))]
public class CommentEditor : Editor {

    private Comment targetComponent;

    private void OnEnable() {
        this.targetComponent = (Comment) this.target;
    }

    public override void OnInspectorGUI() {
        if (this.targetComponent.Text == null) {
            this.targetComponent.Text = ""; // use an empty string so that it won't throw null pointer exception
        }

        this.targetComponent.Text =
            GUILayout.TextArea(this.targetComponent.Text, GUILayout.Height(100), GUILayout.MinWidth(200));
    }
}