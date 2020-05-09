using System.Collections.Generic;

using Common.Math;

using UnityEditor;

using UnityEngine;

namespace Common {
    /// <summary>
    ///     Utility class that renders a certain named variable for use in editor
    /// </summary>
    public class VariableFieldRenderer {
        public delegate object FieldRenderer(ValueHolder holder);

        private readonly Dictionary<NamedValueType, FieldRenderer> fieldRendererMap;

        private readonly int fieldWidth;

        /// <summary>
        ///     Constructor
        /// </summary>
        public VariableFieldRenderer(int fieldWidth) {
            this.fieldWidth = fieldWidth;

            // populate
            this.fieldRendererMap = new Dictionary<NamedValueType, FieldRenderer>();
            AddFieldRenderer(NamedValueType.STRING, delegate(ValueHolder holder) {
                string value = (string) holder.Get();
                value = string.IsNullOrEmpty(value) ? "" : value; // avoid null value
                value = EditorGUILayout.TextField(value, GUILayout.Width(this.fieldWidth)).Trim();

                return value;
            });

            AddFieldRenderer(NamedValueType.INT, delegate(ValueHolder holder) {
                int value = (int) holder.Get();
                value = EditorGUILayout.IntField(value, GUILayout.Width(this.fieldWidth));

                return value;
            });

            AddFieldRenderer(NamedValueType.FLOAT, delegate(ValueHolder holder) {
                float value = (float) holder.Get();
                value = EditorGUILayout.FloatField(value, GUILayout.Width(this.fieldWidth));

                return value;
            });

            AddFieldRenderer(NamedValueType.BOOL, delegate(ValueHolder holder) {
                bool value = (bool) holder.Get();
                value = EditorGUILayout.Toggle(value, GUILayout.Width(20)); // Width of radio button is small

                return value;
            });

            AddFieldRenderer(NamedValueType.VECTOR3, delegate(ValueHolder holder) {
                Vector3 value = (Vector3) holder.Get();
                value = EditorGUILayout.Vector3Field("", value, GUILayout.Width(this.fieldWidth));

                return value;
            });

            AddFieldRenderer(NamedValueType.INT_VECTOR2, delegate(ValueHolder holder) {
                IntVector2 value = (IntVector2) holder.Get();

                EditorGUILayout.BeginHorizontal(GUILayout.Width(110));

                EditorGUILayout.LabelField("x:", GUILayout.Width(15));
                value.x = EditorGUILayout.IntField(value.x, GUILayout.Width(40));

                EditorGUILayout.LabelField("y:", GUILayout.Width(15));
                value.y = EditorGUILayout.IntField(value.y, GUILayout.Width(40));

                EditorGUILayout.EndHorizontal();

                return value;
            });
        }

        private void AddFieldRenderer(NamedValueType type, FieldRenderer renderer) {
            this.fieldRendererMap[type] = renderer;
        }

        /// <summary>
        ///     Renders a field of a specified type
        /// </summary>
        /// <param name="namedType"></param>
        /// <param name="name"></param>
        /// <param name="holder"></param>
        public void Render(NamedValueType namedType, string name, ValueHolder holder) {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(name + ": ", GUILayout.Width(this.fieldWidth));

            Assertion.Assert(this.fieldRendererMap.TryGetValue(namedType, out FieldRenderer fieldRenderer));
            if (fieldRenderer != null) {
                holder.Set(fieldRenderer(holder));
            }

            EditorGUILayout.EndHorizontal();
        }

        public FieldRenderer GetFieldRenderer(NamedValueType valueType) {
            return this.fieldRendererMap[valueType];
        }
    }
}