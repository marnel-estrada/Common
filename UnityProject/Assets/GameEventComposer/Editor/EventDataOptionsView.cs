using System.Collections.Generic;

using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventDataOptionsView {
        private readonly EditorWindow parent;

        public EventDataOptionsView(EditorWindow parent) {
            this.parent = parent;
        }

        public void Render(DataPool<EventData> pool, EventData eventItem) {
            RenderNewOptionSection(pool, eventItem);

            GUILayout.Space(30);
            
            RenderOptions(pool, eventItem);
        }

        private bool showNewOption = true; 
        private string nameId = "";
        private string descriptionId = "";
        private string comment = "";

        private void RenderNewOptionSection(DataPool<EventData> pool, EventData eventItem) {
            this.showNewOption = EditorGUILayout.BeginFoldoutHeaderGroup(this.showNewOption, "New Option");
            
            GUILayout.Space(5);

            if (this.showNewOption) {
                // Name ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name ID:", GUILayout.Width(200));
                this.nameId = EditorGUILayout.TextField(this.nameId, GUILayout.Width(300));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Description ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Description ID:", GUILayout.Width(200));
                this.descriptionId = EditorGUILayout.TextField(this.descriptionId, GUILayout.Width(300));
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
                    AddNewOption(eventItem);
                }
                GUI.backgroundColor = ColorUtils.WHITE;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void AddNewOption(EventData eventItem) {
            // Must have a nameId
            if (string.IsNullOrEmpty(this.nameId)) {
                EditorUtility.DisplayDialog("Add Option", "Option should have a Name ID", "OK");
                return;
            }
            
            int optionId = eventItem.GenerateNewOptionId();
            OptionData option = new OptionData(optionId);
            option.NameId = this.nameId;
            option.DescriptionId = this.descriptionId;
            option.Comment = this.comment;
            
            eventItem.Options.Add(option);
            
            EditorUtility.DisplayDialog("Add Option", $"Option {this.nameId} was added!", "OK");
            
            // Clear the values
            this.nameId = "";
            this.descriptionId = "";
            this.comment = "";
        }

        private void RenderOptions(DataPool<EventData> pool, EventData eventItem) { 
            List<OptionData> options = eventItem.Options;
            
            if (options == null || options.Count == 0) {
                GUILayout.Label("(no options)");
                return;
            }

            for (int i = 0; i < options.Count; ++i) {
                RenderOption(pool, eventItem, options[i]);
                
                GUILayout.Space(30);
            }
        }

        private void RenderOption(DataPool<EventData> pool, EventData eventItem, OptionData option) {
            GUI.backgroundColor = ColorUtils.YELLOW;
            GUILayout.Box(option.NameId, GUILayout.Width(400));
            GUI.backgroundColor = ColorUtils.WHITE;
            
            // Description ID
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description ID: ", GUILayout.Width(150));
            GUILayout.Label(string.IsNullOrEmpty(option.DescriptionId) ? "(no Description ID)" : option.DescriptionId);
            GUILayout.EndHorizontal();
            
            // Child Event
            GUILayout.BeginHorizontal();
            GUILayout.Label("Child Event: ", GUILayout.Width(150));
            GUILayout.Label(ResolveChildEventLabel(pool, option));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            if (!string.IsNullOrEmpty(option.Comment)) {
                EditorGUILayout.HelpBox(option.Comment, MessageType.Info);
                GUILayout.Space(5);
            }
            
            // Buttons here
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit", GUILayout.Width(80))) {
                OpenOptionsWindow(pool, eventItem, option);
            }

            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                PromptDelete(pool, eventItem, option);
            }
            GUI.backgroundColor = ColorUtils.WHITE;
            
            GUILayout.EndHorizontal();
        }

        private string ResolveChildEventLabel(DataPool<EventData> pool, OptionData option) {
            Maybe<EventData> foundEvent = pool.Find(option.ChildEventId);
            if (foundEvent.HasValue) {
                return foundEvent.Value.NameId;
            }
            
            return "(no child event)";
        }

        private void OpenOptionsWindow(DataPool<EventData> pool, EventData eventItem, OptionData option) {
            Rect position = this.parent.position;
            position.x += this.parent.position.width * 0.5f - 200;
            position.y += this.parent.position.height * 0.5f - 300;
            position.width = 400;
            position.height = 600;

            OptionDetailsWindow optionDetailsWindow = ScriptableObject.CreateInstance<OptionDetailsWindow>();
            string optionIdentifier = $"{eventItem.NameId}.{option.NameId}";
            optionDetailsWindow.titleContent = new GUIContent($"Event Option: {optionIdentifier}");
            optionDetailsWindow.Init(this.parent, pool, eventItem, option);
            optionDetailsWindow.position = position;
            optionDetailsWindow.ShowUtility();
            optionDetailsWindow.Focus();
        }

        private void PromptDelete(DataPool<EventData> pool, EventData item, OptionData option) {
            if (!EditorUtility.DisplayDialog("Delete Option", $"Are you sure you want to delete {option.NameId}?",
                "Yes", "No")) {
                // User picked No
                return;
            }
            
            // Proceed with delete
            item.Options.Remove(option);
            EditorUtility.SetDirty(pool);
        }
    }
}