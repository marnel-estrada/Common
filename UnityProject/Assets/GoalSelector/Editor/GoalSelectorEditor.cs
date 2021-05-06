using Common;

using UnityEditor;

using UnityEngine;

namespace GoalSelector.Editor {
    [CustomEditor(typeof(GoalSelectorData))]
    public class GoalSelectorEditor : DataPoolEditor<GoalData> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Editor")) {
                GoalSelectorEditorWindow window = EditorWindow.GetWindow<GoalSelectorEditorWindow>("GoalSelector Editor");
                window.Init(this.DataPool, new GoalDataRenderer(window));
                window.Repaint();
            }
        }
    }
}