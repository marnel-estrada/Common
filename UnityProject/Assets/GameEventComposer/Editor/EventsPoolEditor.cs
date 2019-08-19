using UnityEditor;

using UnityEngine;

namespace GameEvent {
    [CustomEditor(typeof(EventsPool))]
    public class EventsPoolEditor : Editor {
        private EventsPool pool;

        private void OnEnable() {
            this.pool = (EventsPool) this.target;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Open Editor")) {
                EventsEditorWindow window = EditorWindow.GetWindow<EventsEditorWindow>("Events Editor");
                window.Init(this.pool);
                window.Repaint();
            }
        }
    }
}