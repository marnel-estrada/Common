using System;

using Common;

using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    internal class ConditionResolverBrowserWindow : EditorWindow {
        private GUISkin? skin;

        private TypeSelectionRenderer typeSelection;

        private Type selectedType;

        private Action<Type> onSelect;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="onAdd"></param>
        public void Init(Action<Type> onSelect) {
            this.onSelect = onSelect;
            
            // We did it this way so we can search for the skin even when the asset is in the package
            string[] foundResults = AssetDatabase.FindAssets("GoapEditorSkin");
            foreach (string guid in foundResults) {
                this.skin = AssetDatabase.LoadAssetAtPath<GUISkin>(AssetDatabase.GUIDToAssetPath(guid));
                if (this.skin != null) {
                    // Found the skin
                    break;
                }
            }
            Assertion.NotNull(this.skin);

            this.typeSelection = new TypeSelectionRenderer(typeof(ConditionResolverAssembler), this.skin.customStyles[0], OnTypeSelectionChange);
        }

        private void OnTypeSelectionChange(Type type) {
            // Set to selected type
            this.selectedType = type;
        }
        
        private string? filterString;

        private void OnGUI() {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Condition Resolver Browser", EditorStyles.boldLabel);
            GUILayout.Label("Search", EditorStyles.boldLabel);
            this.filterString = GUILayout.TextField(this.filterString);
            
            GUILayout.Space(10);

            this.typeSelection.Render(this.filterString);

            GUILayout.FlexibleSpace();

            GUILayout.Space(5);

            GUILayout.Label("Selected: " + (this.selectedType == null ? "(none)" : this.selectedType.Name));
            if (GUILayout.Button("Select", GUILayout.Width(120))) {
                AddSelectedType();
            }

            EditorGUILayout.EndVertical();
        }

        private void AddSelectedType() {
            if (this.selectedType == null) {
                EditorUtility.DisplayDialog("Add Condition Resolver", "No selected condition resolver", "OK");
                return;
            }

            Close(); // close the window

            this.onSelect(this.selectedType);
        }

        private void Update() {
            // close the window if editor is compiling
            if (EditorApplication.isCompiling) {
                Close();
            }
        }

        private void OnLostFocus() {
            Close();
        }
    }
}
