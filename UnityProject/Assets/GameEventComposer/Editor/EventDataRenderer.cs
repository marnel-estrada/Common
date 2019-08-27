using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventDataRenderer : DataPoolItemRenderer<EventData> {
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(EventData));
        private readonly EventDataRequirementsView requirementsView;

        public EventDataRenderer(EditorWindow parent) {
            this.requirementsView = new EventDataRequirementsView(parent);
        }
        
        public void Render(DataPool<EventData> pool, EventData item) {
            this.renderer.Render(item);
            
            GUILayout.Space(10);
            GUILayout.Label("Requirements", EditorStyles.boldLabel);
            
            this.requirementsView.Render(pool, item);
        }
    }
}