using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common {
    /// <summary>
    /// Common editor for data items that can support components
    /// </summary>
    public class DataComponentsRenderer {
        private readonly EditorWindow parent;

        private Option<Type> selectedComponentDataType;
        private readonly Dictionary<string, GenericObjectRenderer> componentRendererMap = new();
        private Option<GUISkin> skin;
        
        public DataComponentsRenderer(EditorWindow parent) {
            this.parent = parent;
        }
        
        public void RenderComponents(IDataComponentContainer item) {
            GUILayout.Label("Components", EditorStyles.boldLabel);
            
            if (this.selectedComponentDataType.IsSome) {
                // There was a select component data type. We add it to the item.
                item.AddComponent(this.selectedComponentDataType.ValueOrError());
                this.selectedComponentDataType = Option<Type>.NONE; // Consume
            }
            
            // Render existing components
            if (item.HasComponents) {
                RenderExistingComponents(item);
            } else {
                GUILayout.Label("(no components)");
            }
            
            // Add new component button
            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button("Add Component...", GUILayout.Width(120))) {
                // Resolve skin
                if (this.skin.IsNone) {
                    this.skin = Option<GUISkin>.AsOption(ResolveSkin());
                }
                
                Rect position = this.parent.position;
                position.x += (this.parent.position.width * 0.5f) - 200;
                position.y += (this.parent.position.height * 0.5f) - 300;
                position.width = 400;
                position.height = 600;
                
                // Open component browser here
                DataComponentBrowserWindow browserWindow = EditorWindow.GetWindow<DataComponentBrowserWindow>();
                browserWindow.titleContent = new GUIContent("DataComponent Browser");
                browserWindow.Init(this.skin.ValueOrError(), OnAddDataComponent);
                browserWindow.position = new Rect(position);
                browserWindow.ShowUtility();
                browserWindow.Focus();
            }
            
            GUI.backgroundColor = ColorUtils.WHITE; 
        }
        
        private Option<DataComponent> componentToRemove; 

        private void RenderExistingComponents(IDataComponentContainer item) {
            IReadOnlyList<DataComponent> components = item.Components;
            for (int i = 0; i < components.Count; i++) {
                DataComponent component = components[i];
                if (component == null) {
                    GUI.color = Color.red;
                    GUILayout.Label($"There's a null component at index {i}. Remove this using Default GUI Layout.");
                    GUI.color = Color.white;
                    continue;
                }
                
                GUILayout.BeginHorizontal();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                    if (EditorUtility.DisplayDialog("Remove DataComponent", $"Are you sure you want to remove component {component.GetType().FullName}?", "Yes", "No")) {
                        this.componentToRemove = Option<DataComponent>.AsOption(component);
                    }
                }
                GUI.backgroundColor = Color.white;

                GUI.backgroundColor = Color.green;
                GUILayout.Box(component.GetType().Name, GUILayout.Width(250), GUILayout.Height(20));
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                
                // Values
                GetRenderer(component).Render(component); 
            }
            
            // Removal
            if (this.componentToRemove.IsNone) {
                // Nothing to remove
                return;
            }
            
            item.RemoveComponent(this.componentToRemove.ValueOrError());
            this.componentToRemove = Option<DataComponent>.NONE; // Consume
        }

        private GenericObjectRenderer GetRenderer(DataComponent component) {
            Type type = component.GetType();
            if (componentRendererMap.TryGetValue(type.FullName, out GenericObjectRenderer renderer)) {
                // Already created
                return renderer;
            }
            
            // Create a new one
            GenericObjectRenderer newRenderer = new(type);
            componentRendererMap[type.FullName] = newRenderer;
            return newRenderer;
        }
        
        private static GUISkin ResolveSkin() {
            string[] guids = AssetDatabase.FindAssets("CommonEditorSkin t:GUISkin");
            Assertion.IsTrue(guids.Length > 0);

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GUISkin>(path);
        }

        private void OnAddDataComponent(Type dataComponentType) {
            this.selectedComponentDataType = Option<Type>.AsOption(dataComponentType);
        }
    }
}