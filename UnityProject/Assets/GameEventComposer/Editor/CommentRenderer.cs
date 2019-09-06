using System.Reflection;

using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class CommentRenderer : EditorPropertyRenderer {
        public override void Render(PropertyInfo property, object instance) {
            string propValue = (string) property.GetGetMethod().Invoke(instance, null);
            propValue = propValue ?? ""; // Use empty to avoid NullPointerException
            
            GUILayout.Label( property.Name + ":");
            EditorStyles.textField.wordWrap = true;
            propValue = EditorGUILayout.TextArea(propValue, GUILayout.Width(300), GUILayout.Height(80));
            
            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] {
                propValue
            });
        }
    }
}