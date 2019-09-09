using UnityEngine;
using UnityEditor;

namespace Common {
    public static class EditorRenderUtils {

        /// <summary>
        /// Renders a dropdown list or popup
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueSet"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string Dropdown(string value, PopupValueSet valueSet, int width) {
            string finalValue = value ?? "";

            int index = valueSet.ResolveIndex(finalValue);
            if(index < 0) {
                // current value is not found in the value set
                // we use the first entry instead
                index = 0;
            }

            index = EditorGUILayout.Popup(index, valueSet.DisplayList, GUILayout.Width(width));
            return valueSet.GetValue(index);
        }

    }
}
