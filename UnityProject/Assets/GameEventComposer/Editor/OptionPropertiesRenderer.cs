using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionPropertiesRenderer {
        private readonly EditorWindow parent;
        
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;
        
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(OptionData));

        public OptionPropertiesRenderer(EditorWindow parent, DataPool<EventData> pool, EventData eventItem, OptionData option) {
            this.parent = parent;
            
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
            
            GUILayout.Box(ResolveChildEventLabel());
            
            GUILayout.Space(5);

            if (GUILayout.Button("Choose Event...", GUILayout.Width(130))) {
                OpenEventsBrowser();
            }
            
            GUILayout.EndHorizontal();
        }

        private string ResolveChildEventLabel() {
            Maybe<EventData> result = this.pool.Find(this.option.ChildEventId);
            if (result.HasValue) {
                return result.Value.NameId;
            }
            
            return "(no child event)";
        }

        private void OpenEventsBrowser() {
            Rect position = this.parent.position;
            position.x += this.parent.position.width * 0.5f - 200;
            position.y += this.parent.position.height * 0.5f - 300;
            position.width = 400;
            position.height = 600;

            EventsBrowserWindow window = ScriptableObject.CreateInstance<EventsBrowserWindow>();
            window.titleContent = new GUIContent("Events Browser");
            window.Init(this.parent, this.pool, this.eventItem, this.option);
            window.position = position;
            window.ShowUtility();
            window.Focus();
        }
    }
}