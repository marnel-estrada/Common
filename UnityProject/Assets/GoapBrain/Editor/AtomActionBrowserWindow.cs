using System;
using Common;
using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    internal class AtomActionBrowserWindow : EditorWindow {
        private GUISkin? skin;

        private TypeSelectionRenderer? typeSelection;

        private Type? selectedType;

        private Action<Type>? onAdd;
        private string? filterString;
        private bool isOpen;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="onAdd"></param>
        public void Init(Action<Type> onAdd) {
            this.onAdd = onAdd;
            this.isOpen = true;

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

            if (this.skin == null) {
                return;
            }

            this.typeSelection = new TypeSelectionRenderer(typeof(AtomActionAssembler), this.skin.customStyles[0], OnTypeSelectionChange);
        }

        private void OnTypeSelectionChange(Type type) {
            // Set to selected type
            this.selectedType = type;
        }

        private void OnGUI() {
            if (this.typeSelection == null) {
                return;
            }

            GUILayout.BeginVertical();

            GUILayout.Label("Search", EditorStyles.boldLabel);
            this.filterString = GUILayout.TextField(this.filterString);
            
            GUILayout.Space(10);

            this.typeSelection.Render(this.filterString);

            GUILayout.FlexibleSpace();

            GUILayout.Space(5);

            GUILayout.Label("Selected: " + (this.selectedType == null ? "(none)" : this.selectedType.Name));
            if (GUILayout.Button("Add Atom Action", GUILayout.Width(120))) {
                AddSelectedType();
            }

            GUILayout.EndVertical();
        }

        private void AddSelectedType() {
            if (this.onAdd == null) {
                return;
            }

            if (this.selectedType == null) {
                EditorUtility.DisplayDialog("Add Atomic Action", "No selected atomic action", "OK");
                return;
            }

            Close(); // close the window

            this.onAdd(this.selectedType);
        }

        private void Update() {
            // close the window if editor is compiling
            if (EditorApplication.isCompiling) {
                Close();
            }
        }

        private void OnLostFocus() {
            if (!this.isOpen) {
                // Window is already closed. This can be called multiple times if this IMGUI was called inside UIElements.
                return;
            }

            this.isOpen = false;
            Close();
        }
    }
}