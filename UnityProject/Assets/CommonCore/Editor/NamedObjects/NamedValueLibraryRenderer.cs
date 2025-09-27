using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Common {
    /// <summary>
    ///     A utility class that renders the variables of an event
    /// </summary>
    public class NamedValueLibraryRenderer {

        private const int LABEL_WIDTH = 200;
        private const int VALUE_WIDTH = 200;

        private readonly Dictionary<NamedValueType, VariableEntryRenderer> entryRendererMap = new();

        private readonly VariableFieldRenderer fieldRenderer;

        // used for new variable
        private string newVariableName = "";
        private string newVariableType = "";

        private readonly Dictionary<NamedValueType, SimpleList<string>> removalMap = new();

        private PopupValueSet variableTypeValues;

        /// <summary>
        ///     Constructor
        /// </summary>
        public NamedValueLibraryRenderer() {
            PreparePopupValues();

            this.fieldRenderer = new VariableFieldRenderer(250);

            // Populate removalMap and entryRendererMap
            for (int i = 0; i < NamedValueType.ALL_TYPES.Length; ++i) {
                this.removalMap[NamedValueType.ALL_TYPES[i]] = new SimpleList<string>();
                this.entryRendererMap[NamedValueType.ALL_TYPES[i]] =
                    new VariableEntryRenderer(NamedValueType.ALL_TYPES[i], this.removalMap, this.fieldRenderer);
            }
        }

        private void PreparePopupValues() {
            // Prepare popup values
            // Note here that we need an array for the display and value
            List<string> popupTypeDisplay = new() { "(empty)" };
            AddTypeNames(popupTypeDisplay);

            List<string> popupTypeValues = new();
            popupTypeValues.Add(""); // The empty value
            AddTypeNames(popupTypeValues);

            this.variableTypeValues = new PopupValueSet(popupTypeDisplay.ToArray(), popupTypeValues.ToArray());
        }

        private static void AddTypeNames(List<string> list) {
            for (int i = 0; i < NamedValueType.ALL_TYPES.Length; ++i) {
                list.Add(NamedValueType.ALL_TYPES[i].ValueTypeLabel);
            }
        }

        /// <summary>
        ///     Renders the event's variables
        /// </summary>
        /// <param name="eventData"></param>
        public void Render(NamedValueLibrary library) {
            GUILayout.BeginVertical();

            RenderAddVariable(library);

            GUILayout.Space(10);

            // Existing variables
            for (int i = 0; i < NamedValueType.ALL_TYPES.Length; ++i) {
                NamedValueType type = NamedValueType.ALL_TYPES[i];
                RenderVariableList(library.GetContainer(type), type);
                GUILayout.Space(5);
            }

            GUILayout.EndVertical();
        }

        private void RenderAddVariable(NamedValueLibrary library) {
            // name
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name: ", GUILayout.Width(50));
            this.newVariableName = EditorGUILayout.TextField(this.newVariableName, GUILayout.Width(VALUE_WIDTH));
            EditorGUILayout.EndHorizontal();

            // type
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type: ", GUILayout.Width(50));
            this.newVariableType = EditorRenderUtils.Dropdown(this.newVariableType, this.variableTypeValues, 100);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Variable", GUILayout.Width(100))) {
                AddVariable(library);
            }
        }

        private void AddVariable(NamedValueLibrary library) {
            if (string.IsNullOrEmpty(this.newVariableType)) {
                EditorUtility.DisplayDialog("Add Variable", "Can't add variable. No variable type was specified.",
                    "OK");

                return;
            }

            NamedValueContainer container = library.GetContainer(ResolveTypeForNewVariable());
            if (container.Contains(this.newVariableName)) {
                EditorUtility.DisplayDialog("Add Variable", "Can't add variable. Variable already exists.", "OK");

                return;
            }

            container.Add(this.newVariableName);

            this.newVariableName = ""; // empty to avoid confusion
        }

        private NamedValueType ResolveTypeForNewVariable() {
            for (int i = 0; i < NamedValueType.ALL_TYPES.Length; ++i) {
                if (NamedValueType.ALL_TYPES[i].ValueTypeLabel.Equals(this.newVariableType)) {
                    return NamedValueType.ALL_TYPES[i];
                }
            }

            Assertion.IsTrue(false, "Can't resolve type for " + this.newVariableType);

            return default;
        }

        private void RenderVariableList(NamedValueContainer container, NamedValueType type) {
            // Remove the "Named" part
            EditorGUILayout.LabelField(type.ValueTypeLabel, EditorStyles.boldLabel);

            if (container.Count == 0) {
                GUILayout.Label("(empty)");
            } else {
                SimpleList<string> removalList = this.removalMap[type];
                removalList.Clear();

                VariableEntryRenderer entryRenderer = this.entryRendererMap[type];
                for (int i = 0; i < container.Count; ++i) {
                    entryRenderer.Render(container.GetNamedValueHolderAt(i), type);
                }

                // remove variables that are in removal list
                for (int i = 0; i < removalList.Count; ++i) {
                    container.Remove(removalList[i]);
                }

                removalList.Clear();
            }
        }
    }
}