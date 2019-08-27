using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Common {
    /// <summary>
    /// A generic object renderer for editor
    /// </summary>
    public class GenericObjectRenderer {
        private Type type;
        private readonly PropertyInfo[] properties;

        private delegate void PropertyRenderer(PropertyInfo property, object instance);
        private static readonly Dictionary<Type, PropertyRenderer> RENDERER_MAP = new Dictionary<Type, PropertyRenderer>() {
            { typeof(string), RenderString },
            { typeof(int), RenderInt },
            { typeof(float), RenderFloat },
            { typeof(bool), RenderBool },
            { typeof(Vector2), RenderVector2 },
            { typeof(Color), RenderColor }
        };

        private readonly Dictionary<string, List<PropertyInfo>> groupMap = new Dictionary<string, List<PropertyInfo>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericObjectRenderer(Type type) {
            this.type = type;
            this.properties = this.type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private readonly List<PropertyInfo> ungroupedList = new List<PropertyInfo>();

        /// <summary>
        /// Renders an instance of the type
        /// </summary>
        /// <param name="instance"></param>
        public void Render(object instance) {
            ClearGroupedLists();
            this.ungroupedList.Clear();

            // Collect eligible properties first
            foreach (PropertyInfo property in this.properties) {
                // Checks if property is public and has Get and Set methods
                if (!TypeUtils.IsVariableProperty(property)) {
                    continue;
                }

                if (property.Name.ToLower().Equals("id")) {
                    // Do not include ID
                    // Can't be editable
                    continue;
                }

                // Must have a renderer
                PropertyRenderer renderer = RENDERER_MAP.Find(property.PropertyType);
                if(renderer == null) {
                    // No renderer
                    return;
                }

                // At this point the property has a renderer
                // We check if it has a property group
                PropertyGroup group = TypeUtils.GetCustomAttribute<PropertyGroup>(property);
                if(group == null) {
                    // No group. Just add to ungrouped list
                    this.ungroupedList.Add(property);
                } else {
                    // Add to grouped list
                    ResolveGroupedList(group.Name).Add(property);
                }
            }

            // Render the ungrouped ones first
            RenderProperties(this.ungroupedList, instance);

            // Render each grouped list
            foreach(KeyValuePair<string, List<PropertyInfo>> entry in this.groupMap) {
                GUILayout.Space(10);
                GUILayout.Label(entry.Key, EditorStyles.boldLabel);
                RenderProperties(entry.Value, instance);
            }
        }

        private void ClearGroupedLists() {
            foreach (KeyValuePair<string, List<PropertyInfo>> entry in this.groupMap) {
                entry.Value.Clear();
            }
        }

        private static void RenderProperties(List<PropertyInfo> propertyList, object instance) {
            // Sort
            propertyList.Sort(AscendingNameComparison);

            // Render each property
            for(int i = 0; i < propertyList.Count; ++i) {
                PropertyInfo property = propertyList[i];
                ReadOnlyFieldAttribute readOnlyAttribute = TypeUtils.GetCustomAttribute<ReadOnlyFieldAttribute>(property);

                if (readOnlyAttribute == null) {
                    // It's an editable field
                    RENDERER_MAP[property.PropertyType](property, instance); // Invoke the renderer
                } else {
                    // It's readonly
                    RenderReadOnly(property, instance);
                }
                
                GUILayout.Space(5);
            }
        }

        private static int AscendingNameComparison(PropertyInfo a, PropertyInfo b) {
            return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        private List<PropertyInfo> ResolveGroupedList(string groupName) {
            List<PropertyInfo> list = this.groupMap.Find(groupName);
            if(list != null) {
                // Already exists
                return list;
            }

            // Create a new one since it doesn't exist yet
            list = new List<PropertyInfo>();
            this.groupMap[groupName] = list;
            return list;
        }

        private static void RenderReadOnly(PropertyInfo property, object instance) {
            string value = property.GetGetMethod().Invoke(instance, null).ToString();
            value = string.IsNullOrEmpty(value) ? "" : value; // Prevent null
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            GUILayout.Label(value, GUILayout.Width(300));
            GUILayout.EndHorizontal();
        }

        private static void RenderString(PropertyInfo property, object instance) {
            string value = property.GetGetMethod().Invoke(instance, null) as string;
            value = string.IsNullOrEmpty(value) ? "" : value; // Prevent null

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.TextField(value, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderInt(PropertyInfo property, object instance) {
            int value = (int)property.GetGetMethod().Invoke(instance, null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.IntField(value, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderFloat(PropertyInfo property, object instance) {
            float value = (float)property.GetGetMethod().Invoke(instance, null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.FloatField(value, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderBool(PropertyInfo property, object instance) {
            bool value = (bool)property.GetGetMethod().Invoke(instance, null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.Toggle(value);
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderVector2(PropertyInfo property, object instance) {
            Vector2 value = (Vector2)property.GetGetMethod().Invoke(instance, null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.Vector2Field("", value, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderColor(PropertyInfo property, object instance) {
            Color value = (Color)property.GetGetMethod().Invoke(instance, null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.ColorField("", value, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }
    }
}
