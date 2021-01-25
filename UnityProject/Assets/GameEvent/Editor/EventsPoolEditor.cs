using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    [CustomEditor(typeof(EventsPool))]
    public class EventsPoolEditor : DataPoolEditor<EventData> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Open Editor")) {
                EventsEditorWindow window = EditorWindow.GetWindow<EventsEditorWindow>("Events Editor");
                window.Init(this.DataPool, new EventDataRenderer(window));
                window.AddFilterStrategy(new OptionCostFilterStrategy(90));
                window.Repaint();
            }
        }
    }
}