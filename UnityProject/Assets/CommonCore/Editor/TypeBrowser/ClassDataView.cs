using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Common {
    /// <summary>
    /// A generic view that renders a list of ClassData instances
    /// </summary>
    public class ClassDataView<ClassDataType, TypeBrowserType> where TypeBrowserType : TypeBrowserWindow<ClassDataType> {
        private readonly EditorWindow parent;
        private readonly Signal.Signal repaintSignal;
        private readonly string classDataTypeName;
        
        private Object dataSource;
        private GUISkin skin;
        
        private readonly ClassPropertiesRenderer propertiesRenderer = new ClassPropertiesRenderer(200);

        private List<ClassData> dataList;

        public ClassDataView(EditorWindow parent, Signal.Signal repaintSignal) {
            this.parent = parent;
            this.repaintSignal = repaintSignal;
            this.classDataTypeName = typeof(ClassDataType).Name;
        }

        public void Render(Object dataSource, List<ClassData> dataList, GUISkin skin) {
            this.dataSource = dataSource;
            this.dataList = dataList;
            this.skin = skin;
            
            // Add new
            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button($"Add {this.classDataTypeName}...", GUILayout.Width(140))) {
                OpenTypeBrowser();
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            // Render all class data
            if (dataList == null || dataList.Count <= 0) {
                GUILayout.Label($"(no {this.classDataTypeName.ToLowerInvariant()} yet)");
            } else {
                RenderAllClassData();
            }
        }
        
        private void OpenTypeBrowser() {
            Rect position = this.parent.position;
            position.x += this.parent.position.width * 0.5f - 200;
            position.y += this.parent.position.height * 0.5f - 300;
            position.width = 400;
            position.height = 600;

            TypeBrowserType browserWindow =
                ScriptableObject.CreateInstance<TypeBrowserType>();
            browserWindow.titleContent = new GUIContent($"{this.classDataTypeName} Browser");
            browserWindow.Init(this.skin, OnAdd);
            browserWindow.position = position;
            browserWindow.ShowUtility();
            browserWindow.Focus();
        }
        
        private void OnAdd(Type type) {
            ClassData classData = new ClassData();
            classData.ClassName = type.FullName;

            this.dataList.Add(classData);

            EditorUtility.SetDirty(this.dataSource);
            this.repaintSignal.Dispatch();
        }
        
        private void RenderAllClassData() {
            for (int i = 0; i < this.dataList.Count; ++i) {
                RenderClassData(this.dataList[i], i);
                GUILayout.Space(5);
            }
        }
        
        private void RenderClassData(ClassData data, int index) {
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

            // Up button 
            if (GUILayout.Button("Up", GUILayout.Width(25), GUILayout.Height(20))) {
                MoveUp(index);
            }

            // Down button
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
            if (EditorUtility.DisplayDialogComplex($"Remove {this.classDataTypeName}",
                $"Are you sure you want to remove this {this.classDataTypeName.ToLowerInvariant()} {data.ClassName}?", "Yes", "No", "Cancel") != 0) {
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