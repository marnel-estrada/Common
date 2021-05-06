using System;

using UnityEditor;

using UnityEngine;

namespace Common {
    /// <summary>
    /// A generic browser window that can support any base type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeBrowserWindow<T> : EditorWindow {
        private Action<Type> onAdd;

        private Type selectedType;
        private GUISkin skin;

        private TypeSelectionRenderer typeSelection;

        private string baseTypeName;
        
        /// <summary>
        ///     Initializer
        /// </summary>
        /// <param name="onAdd"></param>
        public void Init(GUISkin skin, Action<Type> onAdd) {
            this.skin = skin;
            this.onAdd = onAdd;
            
            Assertion.NotNull(this.skin);

            this.typeSelection =
                new TypeSelectionRenderer(typeof(T), this.skin.customStyles[0], OnTypeSelectionChange);

            this.baseTypeName = typeof(T).Name;
        }

        private void OnTypeSelectionChange(Type type) {
            // Set to selected type
            this.selectedType = type;
        }

        private void OnGUI() {
            EditorGUILayout.BeginVertical();

            GUILayout.Label($"{this.baseTypeName} Browser", EditorStyles.boldLabel);
            GUILayout.Space(10);

            this.typeSelection.Render();

            GUILayout.FlexibleSpace();

            GUILayout.Space(5);

            GUILayout.Label("Selected: " + (this.selectedType == null ? "(none)" : this.selectedType.Name));
            if (GUILayout.Button($"Add {this.baseTypeName}", GUILayout.Width(300))) {
                AddSelectedType();
            }

            EditorGUILayout.EndVertical();
        }

        private void AddSelectedType() {
            if (this.selectedType == null) {
                EditorUtility.DisplayDialog($"Add {this.baseTypeName}", $"No selected {this.baseTypeName}", "OK");
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