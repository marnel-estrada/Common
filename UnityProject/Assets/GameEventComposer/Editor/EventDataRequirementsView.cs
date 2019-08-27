using System;

using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventDataRequirementsView {
        private readonly EditorWindow parent;
        private readonly ClassPropertiesRenderer propertiesRenderer;

        private DataPool<EventData> pool;
        private EventData item;

        public EventDataRequirementsView(EditorWindow parent) {
            this.parent = parent;
            this.propertiesRenderer = new ClassPropertiesRenderer(200);
        }
        
        public void Render(DataPool<EventData> pool, EventData item) {
            this.pool = pool;
            this.item = item;
            
            // Add new
            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button("Add Requirement...", GUILayout.Width(140))) {
                OpenRequirementsBrowser();
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            // Render requirements
            if (item.Requirements.Count <= 0) {
                GUILayout.Label("(no requirements yet)");
            } else {
                //RenderRequirements(grantsData, grant);
            }
        }
        
        private void OpenRequirementsBrowser() {
            Rect position = this.parent.position;
            position.x += this.parent.position.width * 0.5f - 200;
            position.y += this.parent.position.height * 0.5f - 300;
            position.width = 400;
            position.height = 600;

            RequirementsBrowserWindow requirementBrowser =
                ScriptableObject.CreateInstance<RequirementsBrowserWindow>();
            requirementBrowser.titleContent = new GUIContent("Requirements Browser");
            requirementBrowser.Init(this.pool.Skin, OnAdd);
            requirementBrowser.position = position;
            requirementBrowser.ShowUtility();
            requirementBrowser.Focus();
        }
        
        private void OnAdd(Type type) {
            ClassData classData = new ClassData();
            classData.ClassName = type.FullName;

            this.item.Requirements.Add(classData);

            EditorUtility.SetDirty(this.pool);
            DataPoolEditorWindow<EventData>.REPAINT.Dispatch();
        }
    }
}