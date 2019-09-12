using UnityEditor;

using UnityEngine;

namespace Test {
    [CustomEditor(typeof(DummyObject))]
    public class DummyObjectEditor : GenericEditor<DummyObject> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            GUILayout.Label($"Data: {((DummyObject) this.target).data}");
        }
    }
}