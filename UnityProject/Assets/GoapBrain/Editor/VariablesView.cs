using Common;

using UnityEditor;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Renders the variables part of the GOAP editor
    /// </summary>
    public class VariablesView {
        private readonly NamedValueLibraryRenderer variablesRenderer = new();

        private Vector2 scrollPosition;

        /// <summary>
        /// Renders the UI
        /// </summary>
        /// <param name="domain"></param>
        public void Render(GoapDomainData domain) {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Variables", EditorStyles.boldLabel);
            GUILayout.Space(10);

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.variablesRenderer.Render(domain.Variables);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
