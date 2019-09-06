using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionPropertiesRenderer {
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;
        
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(OptionData));

        public OptionPropertiesRenderer(DataPool<EventData> pool, EventData eventItem, OptionData option) {
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
        }

        public void Render() {
            this.renderer.Render(this.option);
            
            GUILayout.Space(10);
            
            // Child Event
            GUILayout.Label("Child Event", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Child Event:", GUILayout.Width(150));

            if (GUILayout.Button("Choose Event...", GUILayout.Width(130))) {
                Debug.Log("Choose Event");
            }
            
            GUILayout.EndHorizontal();
        }
    }
}