using System.Reflection;

using Common;

using UnityEngine;

namespace GameEvent {
    public class RarityRenderer : EditorPropertyRenderer {
        private static PopupValueSet VALUE_SET;
        
        public override void Render(PropertyInfo property, object instance) {
            PrepareOptions();
            
            int id = (int)property.GetGetMethod().Invoke(instance, null);
            string name = Rarity.ConvertFromId(id).name;

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            name = EditorRenderUtils.Dropdown(name, VALUE_SET, 150);
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { Rarity.ConvertFromName(name).id });
        }

        private static void PrepareOptions() {
            if (VALUE_SET != null) {
                // Already prepared
                return;
            }
            
            int length = Rarity.ALL.Length;
            string[] options = new string[length];
            for (int i = 0; i < length; ++i) {
                options[i] = Rarity.ALL[i].name;
            }
            
            VALUE_SET = new PopupValueSet(options, options);
        }
    }
}