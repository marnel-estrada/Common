using Common;

using UnityEditor;

using UnityEngine;

namespace Test {
    [CustomEditor(typeof(TestDataPool))]
    public class TestDataPoolEditor : DataPoolEditor<TestData> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        
            if (GUILayout.Button("Open Editor")) {
                TestDataPoolEditorWindow window = EditorWindow.GetWindow<TestDataPoolEditorWindow>("Test Data Editor");
                window.Init(this.DataPool, new TestDataItemRenderer());
                window.Repaint();
            }
        }
    }
}