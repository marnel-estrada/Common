using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventDataOptionsView {
        private readonly EditorWindow parent;
        
        private DataPool<EventData> pool;
        private EventData item;

        public EventDataOptionsView(EditorWindow parent) {
            this.parent = parent;
        }

        public void Render(DataPool<EventData> pool, EventData item) {
            RenderNewOptionSection(pool, item);
        }

        private bool showNewOption = true; 
        private string nameId = "";
        private string descriptionId = "";

        private void RenderNewOptionSection(DataPool<EventData> pool, EventData item) {
            this.showNewOption = EditorGUILayout.BeginFoldoutHeaderGroup(this.showNewOption, "New Option");
            
            GUILayout.Space(5);

            if (this.showNewOption) {
                // Name ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name ID:", GUILayout.Width(150));
                this.nameId = EditorGUILayout.TextField(this.nameId, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Description ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Description ID:", GUILayout.Width(150));
                this.descriptionId = EditorGUILayout.TextField(this.descriptionId, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Add button
                GUI.backgroundColor = ColorUtils.GREEN;
                if (GUILayout.Button("Add", GUILayout.Width(80))) {
                    Debug.Log("Add Option!");
                }
                GUI.backgroundColor = ColorUtils.WHITE;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}