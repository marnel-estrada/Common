using System;
using System.Collections.Generic;

using Common;
using Common.Signal;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace GameEvent {
    public class RequirementsView {
        private readonly EditorWindow parent;
        private readonly Signal repaintSignal;
        
        private Object dataSource;
        private GUISkin skin;
        
        private readonly ClassPropertiesRenderer propertiesRenderer = new ClassPropertiesRenderer(200);

        private List<ClassData> dataList;

        public RequirementsView(EditorWindow parent, Signal repaintSignal) {
            this.parent = parent;
            this.repaintSignal = repaintSignal;
        }

        public void Render(Object dataSource, List<ClassData> dataList, GUISkin skin) {
            this.dataSource = dataSource;
            this.dataList = dataList;
            this.skin = skin;
            
            // Add new
            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button("Add Requirement...", GUILayout.Width(140))) {
                OpenRequirementsBrowser();
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            // Render requirements
            if (dataList == null || dataList.Count <= 0) {
                GUILayout.Label("(no requirements yet)");
            } else {
                RenderRequirements();
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
            requirementBrowser.Init(this.skin, OnAdd);
            requirementBrowser.position = position;
            requirementBrowser.ShowUtility();
            requirementBrowser.Focus();
        }
        
        private void OnAdd(Type type) {
            ClassData classData = new ClassData();
            classData.ClassName = type.FullName;

            this.dataList.Add(classData);

            EditorUtility.SetDirty(this.dataSource);
            this.repaintSignal.Dispatch();
        }
        
        private void RenderRequirements() {
            for (int i = 0; i < this.dataList.Count; ++i) {
                RenderRequirement(this.dataList[i], i);
                GUILayout.Space(5);
            }
        }
        
        private void RenderRequirement(ClassData data, int index) {
            if (data.ClassType == null) {
                // Cache
                data.ClassType = TypeUtils.GetType(data.ClassName);
                Assertion.AssertNotNull(data.ClassType);
            }

            GUILayout.BeginHorizontal();

            // delete button
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                Remove(data);
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            // up button 
            if (GUILayout.Button("Up", GUILayout.Width(25), GUILayout.Height(20))) {
                MoveUp(index);
            }

            // down button
            if (GUILayout.Button("Down", GUILayout.Width(45), GUILayout.Height(20))) {
                MoveDown(index);
            }

            GUILayout.Box(data.ClassType.Name);

            GUILayout.EndHorizontal();

            // Variables
            Assertion.AssertNotNull(data.ClassType);
            this.propertiesRenderer.RenderVariables(data.Variables, data.Variables, data.ClassType, data.ShowHints);
        }
        
        private void Remove(ClassData data) {
            if (EditorUtility.DisplayDialogComplex("Remove Requirement",
                $"Are you sure you want to remove this requirement {data.ClassName}?", "Yes", "No", "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            this.dataList.Remove(data);

            EditorUtility.SetDirty(this.dataSource);
            this.repaintSignal.Dispatch();
        }

        private void MoveUp(int index) {
            if (index <= 0) {
                // Can no longer move up
                return;
            }

            Swap(index, index - 1);

            EditorUtility.SetDirty(this.dataSource);
            this.repaintSignal.Dispatch();
        }

        private void MoveDown(int index) {
            if (index + 1 >= this.dataList.Count) {
                // Can no longer move down
                return;
            }

            Swap(index, index + 1);

            EditorUtility.SetDirty(this.dataSource);
            this.repaintSignal.Dispatch();
        }

        private void Swap(int a, int b) {
            ClassData temp = this.dataList[a];
            this.dataList[a] = this.dataList[b];
            this.dataList[b] = temp;
        }
    }
}