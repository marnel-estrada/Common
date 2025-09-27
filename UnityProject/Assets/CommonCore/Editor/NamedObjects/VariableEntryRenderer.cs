using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Common {
    internal class VariableEntryRenderer {
        private readonly VariableFieldRenderer fieldRenderer;

        private readonly Dictionary<NamedValueType, SimpleList<string>> removalMap;
        private readonly NamedValueType type;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="addAction"></param>
        /// <param name="removeAction"></param>
        public VariableEntryRenderer(NamedValueType type, Dictionary<NamedValueType, SimpleList<string>> removalMap,
            VariableFieldRenderer fieldRenderer) {
            this.type = type;
            this.removalMap = removalMap;
            this.fieldRenderer = fieldRenderer;
        }

        /// <summary>
        /// </summary>
        /// <param name="named"></param>
        /// <param name="holder"></param>
        public void Render(NamedValueHolder valueHolder, NamedValueType type) {
            EditorGUILayout.BeginHorizontal();

            // close button
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) {
                if (EditorUtility.DisplayDialog("Remove Variable",
                        $"Are you sure you want to remove variable \"{valueHolder.Name}\"?", "Yes",
                    "No")) {
                    this.removalMap[this.type].Add(valueHolder.Name); // Add to removal list
                }
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            this.fieldRenderer.Render(type, valueHolder.Name, valueHolder);

            EditorGUILayout.EndHorizontal();
        }
    }
}