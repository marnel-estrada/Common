using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class ProbabilityRenderer : EditorPropertyRenderer {
        public override void Render(PropertyInfo property, object instance) {
            int value = (int) property.GetGetMethod().Invoke(instance, null);
            
            GUILayout.BeginHorizontal();
            
            // The slider
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.IntSlider(Mathf.RoundToInt(value), 0, 100, GUILayout.Width(200));
            
            GUILayout.Label("%");
            
            GUILayout.EndHorizontal();
            
            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] {
                value
            });
        }
    }
}