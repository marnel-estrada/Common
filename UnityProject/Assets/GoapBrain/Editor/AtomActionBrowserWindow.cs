using System;

using Common;

using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    class AtomActionBrowserWindow : EditorWindow {
        private GUISkin skin;

        private TypeSelectionRenderer typeSelection;

        private Type selectedType;

        private Action<Type> onAdd;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="onAdd"></param>
        public void Init(Action<Type> onAdd) {
            this.onAdd = onAdd;
            
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

            this.typeSelection = new TypeSelectionRenderer(typeof(GoapAtomAction), this.skin.customStyles[0], OnTypeSelectionChange);
        }

        private void OnTypeSelectionChange(Type type) {
            // Set to selected type
            this.selectedType = type;
        }

        private Vector2 scrollPos = new Vector2();

        void OnGUI() {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Atom Actions Browser", EditorStyles.boldLabel);
            GUILayout.Space(10);

            this.typeSelection.Render();

            GUILayout.FlexibleSpace();

            GUILayout.Space(5);

            GUILayout.Label("Selected: " + (this.selectedType == null ? "(none)" : this.selectedType.Name));
            if (GUILayout.Button("Add Atom Action", GUILayout.Width(120))) {
                AddSelectedType();
            }

            EditorGUILayout.EndVertical();
        }

        private void AddSelectedType() {
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
            Close();
        }
    }
}
