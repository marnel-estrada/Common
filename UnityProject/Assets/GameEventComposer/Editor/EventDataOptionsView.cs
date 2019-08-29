using System.Collections.Generic;

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

            GUILayout.Space(30);
            
            RenderOptions(pool, item);
        }

        private bool showNewOption = true; 
        private string nameId = "";
        private string descriptionId = "";
        private string comment = "";

        private void RenderNewOptionSection(DataPool<EventData> pool, EventData item) {
            this.showNewOption = EditorGUILayout.BeginFoldoutHeaderGroup(this.showNewOption, "New Option");
            
            GUILayout.Space(5);

            if (this.showNewOption) {
                // Name ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name ID:", GUILayout.Width(200));
                this.nameId = EditorGUILayout.TextField(this.nameId, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Description ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Description ID:", GUILayout.Width(200));
                this.descriptionId = EditorGUILayout.TextField(this.descriptionId, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Comment
                GUILayout.Label("Comment:");
                EditorStyles.textField.wordWrap = true;
                this.comment = EditorGUILayout.TextArea(this.comment, GUILayout.Width(300), 
                    GUILayout.Height(80));
                
                GUILayout.Space(5);
                
                // Add button
                GUI.backgroundColor = ColorUtils.GREEN;
                if (GUILayout.Button("Add", GUILayout.Width(80))) {
                    AddNewOption(item);
                }
                GUI.backgroundColor = ColorUtils.WHITE;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void AddNewOption(EventData item) {
            // Must have a nameId
            if (string.IsNullOrEmpty(this.nameId)) {
                EditorUtility.DisplayDialog("Add Option", "Option should have a Name ID", "OK");
                return;
            }
            
            int optionId = item.GenerateNewOptionId();
            OptionData option = new OptionData(optionId);
            option.NameId = this.nameId;
            option.DescriptionId = this.descriptionId;
            option.Comment = this.comment;
            
            item.Options.Add(option);
            
            EditorUtility.DisplayDialog("Add Option", $"Option {this.nameId} was added!", "OK");
            
            // Clear the values
            this.nameId = "";
            this.descriptionId = "";
            this.comment = "";
        }

        private void RenderOptions(DataPool<EventData> pool, EventData item) { 
            List<OptionData> options = item.Options;
            
            if (options == null || options.Count == 0) {
                GUILayout.Label("(no options)");
                return;
            }

            for (int i = 0; i < options.Count; ++i) {
                RenderOption(pool, item, options[i]);
                
                GUILayout.Space(30);
            }
        }

        private void RenderOption(DataPool<EventData> pool, EventData item, OptionData option) {
            GUI.backgroundColor = ColorUtils.YELLOW;
            GUILayout.Box(option.NameId, GUILayout.Width(400));
            GUI.backgroundColor = ColorUtils.WHITE;
            
            GUILayout.Space(5);
            
            GUILayout.Label(option.DescriptionId);
            
            GUILayout.Space(5);

            if (!string.IsNullOrEmpty(option.Comment)) {
                EditorGUILayout.HelpBox(option.Comment, MessageType.Info);
                GUILayout.Space(5);
            }
            
            // Buttons here
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit", GUILayout.Width(80))) {
                Debug.Log("Edit Option");
            }

            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                Debug.Log("Delete Option");
            }
            GUI.backgroundColor = ColorUtils.WHITE;
            
            GUILayout.EndHorizontal();
        }
    }
}