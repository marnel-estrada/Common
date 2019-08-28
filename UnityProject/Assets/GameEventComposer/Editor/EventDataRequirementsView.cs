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
                RenderRequirements(pool, item);
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
        
        private void RenderRequirements(DataPool<EventData> pool, EventData item) {
            for (int i = 0; i < item.Requirements.Count; ++i) {
                RenderRequirement(pool, item, item.Requirements[i], i);
                GUILayout.Space(5);
            }
        }
        
        private void RenderRequirement(DataPool<EventData> pool, EventData item, ClassData data, int index) {
            if (data.ClassType == null) {
                // Cache
                data.ClassType = TypeUtils.GetType(data.ClassName);
                Assertion.AssertNotNull(data.ClassType);
            }

            GUILayout.BeginHorizontal();

            // delete button
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                Remove(pool, item, data);
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            // up button 
            if (GUILayout.Button("Up", GUILayout.Width(25), GUILayout.Height(20))) {
                MoveUp(pool, item, index);
            }

            // down button
            if (GUILayout.Button("Down", GUILayout.Width(45), GUILayout.Height(20))) {
                MoveDown(pool, item, index);
            }

            GUILayout.Box(data.ClassType.Name);

            GUILayout.EndHorizontal();

            // Variables
            Assertion.AssertNotNull(data.ClassType);
            this.propertiesRenderer.RenderVariables(data.Variables, data.Variables, data.ClassType, data.ShowHints);
        }
        
        private void Remove(DataPool<EventData> pool, EventData item, ClassData data) {
            if (EditorUtility.DisplayDialogComplex("Remove Requirement",
                "Are you sure you want to remove this requirement?", "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            item.Requirements.Remove(data);

            EditorUtility.SetDirty(pool);
            DataPoolEditorWindow<EventData>.REPAINT.Dispatch();
        }

        private void MoveUp(DataPool<EventData> pool, EventData item, int index) {
            if (index <= 0) {
                // Can no longer move up
                return;
            }

            Swap(item, index, index - 1);

            EditorUtility.SetDirty(pool);
            DataPoolEditorWindow<EventData>.REPAINT.Dispatch();
        }

        private void MoveDown(DataPool<EventData> pool, EventData item, int index) {
            if (index + 1 >= item.Requirements.Count) {
                // Can no longer move down
                return;
            }

            Swap(item, index, index + 1);

            EditorUtility.SetDirty(pool);
            DataPoolEditorWindow<EventData>.REPAINT.Dispatch();
        }

        private static void Swap(EventData item, int a, int b) {
            ClassData temp = item.Requirements[a];
            item.Requirements[a] = item.Requirements[b];
            item.Requirements[b] = temp;
        }
    }
}