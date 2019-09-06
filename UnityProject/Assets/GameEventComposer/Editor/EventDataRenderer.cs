using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventDataRenderer : DataPoolItemRenderer<EventData> {
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(EventData));
        private readonly RequirementsView requirementsView;
        private readonly EventDataOptionsView optionsView;

        public EventDataRenderer(EditorWindow parent) {
            this.requirementsView = new RequirementsView(parent, EventsEditorWindow.REPAINT);
            this.optionsView = new EventDataOptionsView(parent);
        }
        
        private Vector2 scrollPos;
        
        public void Render(DataPool<EventData> pool, EventData item) {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            
            this.renderer.Render(item);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Requirements", EditorStyles.boldLabel);
            this.requirementsView.Render(pool, item.Requirements, pool.Skin);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Options", EditorStyles.boldLabel);
            this.optionsView.Render(pool, item);
            
            GUILayout.EndScrollView();
        }
    }
}