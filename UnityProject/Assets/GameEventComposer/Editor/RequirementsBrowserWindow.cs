using System;

using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class RequirementsBrowserWindow : TypeBrowserWindow<Requirement> {
        // private Action<Type> onAdd;
        //
        // private Type selectedType;
        // private GUISkin skin;
        //
        // private TypeSelectionRenderer typeSelection;
        //
        // /// <summary>
        // ///     Initializer
        // /// </summary>
        // /// <param name="onAdd"></param>
        // public void Init(GUISkin skin, Action<Type> onAdd) {
        //     this.skin = skin;
        //     this.onAdd = onAdd;
        //     
        //     Assertion.AssertNotNull(this.skin);
        //
        //     this.typeSelection =
        //         new TypeSelectionRenderer(typeof(Requirement), this.skin.customStyles[0], OnSelectionChange);
        // }
        //
        // private void OnSelectionChange(Type type) {
        //     // Set to selected type
        //     this.selectedType = type;
        // }
        //
        // private void OnGUI() {
        //     EditorGUILayout.BeginVertical();
        //
        //     GUILayout.Label("Requirements Browser", EditorStyles.boldLabel);
        //     GUILayout.Space(10);
        //
        //     this.typeSelection.Render();
        //
        //     GUILayout.FlexibleSpace();
        //
        //     GUILayout.Space(5);
        //
        //     GUILayout.Label("Selected: " + (this.selectedType == null ? "(none)" : this.selectedType.Name));
        //     if (GUILayout.Button("Add Requirement", GUILayout.Width(120))) {
        //         AddSelectedType();
        //     }
        //
        //     EditorGUILayout.EndVertical();
        // }
        //
        // private void AddSelectedType() {
        //     if (this.selectedType == null) {
        //         EditorUtility.DisplayDialog("Add Requirement", "No selected requirement", "OK");
        //         return;
        //     }
        //
        //     Close(); // close the window
        //
        //     this.onAdd(this.selectedType);
        // }
        //
        // private void Update() {
        //     // close the window if editor is compiling
        //     if (EditorApplication.isCompiling) {
        //         Close();
        //     }
        // }
        //
        // private void OnLostFocus() {
        //     Close();
        // }
    }
}