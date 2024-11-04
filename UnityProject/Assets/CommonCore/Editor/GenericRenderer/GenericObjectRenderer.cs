using System;
using System.Collections.Generic;
using System.Reflection;

using Unity.Mathematics;

using UnityEngine;
using UnityEditor;

namespace Common {
    /// <summary>
    /// A generic object renderer for editor
    /// </summary>
    public class GenericObjectRenderer {
        private readonly Type type;
        private readonly PropertyInfo[] properties;

        private delegate void PropertyRenderer(PropertyInfo property, object instance);
        private static readonly Dictionary<Type, PropertyRenderer> RENDERER_MAP = new() {
            { typeof(string), RenderString },
            { typeof(byte), RenderByte },
            { typeof(int), RenderInt },
            { typeof(float), RenderFloat },
            { typeof(bool), RenderBool },
            { typeof(Vector2), RenderVector2 },
            { typeof(float2), RenderFloat2 },
            { typeof(Color), RenderColor },
            { typeof(List<string>), RenderStringList }
        };

        private readonly Dictionary<string, List<PropertyInfo>> groupMap = new();
        
        private readonly Dictionary<string, EditorPropertyRenderer> customRenderers = new(1);

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericObjectRenderer(Type type) {
            this.type = type;
            this.properties = this.type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private readonly List<PropertyInfo> ungroupedList = new();

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

                if (TypeUtils.GetCustomAttribute<Hidden>(property) != null) {
                    // Do not include properties with Hidden attribute 
                    continue;
                }

                // Must have a renderer
                if (!HasRenderer(property)) {
                    continue;
                }

                // At this point the property has a renderer
                // We check if it has a property group
                PropertyGroup? group = TypeUtils.GetCustomAttribute<PropertyGroup>(property);
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

        private static bool HasRenderer(PropertyInfo property) {
            // Check if property has a custom renderer
            Common.PropertyRenderer? propertyRenderer = TypeUtils.GetCustomAttribute<Common.PropertyRenderer>(property);
            
            if (propertyRenderer == null || string.IsNullOrEmpty(propertyRenderer.RendererType)) {
                // No custom renderer
                Option<PropertyRenderer> renderer = RENDERER_MAP.Find(property.PropertyType);
                return renderer.IsSome;
            }
            
            // Has a custom renderer
            return true;
        }

        private void ClearGroupedLists() {
            foreach (KeyValuePair<string, List<PropertyInfo>> entry in this.groupMap) {
                entry.Value.Clear();
            }
        }
        
        private static readonly GenericObjectPropertyInfoComparer PROPERTY_INFO_COMPARER = new();

        private void RenderProperties(List<PropertyInfo> propertyList, object instance) {
            // Sort
            propertyList.Sort(PROPERTY_INFO_COMPARER);

            // Render each property
            for(int i = 0; i < propertyList.Count; ++i) {
                PropertyInfo property = propertyList[i];
                
                // Check if it has a custom renderer
                Common.PropertyRenderer? propertyRenderer = TypeUtils.GetCustomAttribute<Common.PropertyRenderer>(property);
                if (propertyRenderer == null || string.IsNullOrEmpty(propertyRenderer.RendererType)) {
                    // No renderer type specified
                    RenderAsDefault(property, instance);
                } else {
                    CustomRender(property, instance, propertyRenderer);
                } 

                GUILayout.Space(5);
            }
        }

        private static void RenderAsDefault(PropertyInfo property, object instance) {
            ReadOnlyFieldAttribute? readOnlyAttribute = TypeUtils.GetCustomAttribute<ReadOnlyFieldAttribute>(property);

            if (readOnlyAttribute == null) {
                // It's an editable field
                RENDERER_MAP[property.PropertyType](property, instance); // Invoke the renderer
            } else {
                // It's readonly
                RenderReadOnly(property, instance);
            }
        }
        
        private void CustomRender(PropertyInfo property, object instance, Common.PropertyRenderer propRenderer) {
            Option<EditorPropertyRenderer> resolvedRenderer = ResolveRenderer(propRenderer.RendererType);
            Assertion.IsSome(resolvedRenderer);

            resolvedRenderer.Match(new CustomRenderPropertyMatcher(property, instance));
        }

        private readonly struct CustomRenderPropertyMatcher : IOptionMatcher<EditorPropertyRenderer> {
            private readonly PropertyInfo property;
            private readonly object instance;

            public CustomRenderPropertyMatcher(PropertyInfo property, object instance) {
                this.property = property;
                this.instance = instance;
            }

            public void OnSome(EditorPropertyRenderer renderer) {
                renderer.Render(this.property, instance);
            }

            public void OnNone() {
            }
        }

        private readonly struct CreateEditorPropertyRendererMatcher : IFuncOptionMatcher<Type, Option<EditorPropertyRenderer>> {
            private readonly string rendererTypeName;
            private readonly Dictionary<string, EditorPropertyRenderer> customRenderers;

            public CreateEditorPropertyRendererMatcher(string rendererTypeName, Dictionary<string, EditorPropertyRenderer> customRenderers) {
                this.rendererTypeName = rendererTypeName;
                this.customRenderers = customRenderers;
            }

            public Option<EditorPropertyRenderer> OnSome(Type rendererType) {
                ConstructorInfo constructor = TypeUtils.ResolveEmptyConstructor(rendererType);
                EditorPropertyRenderer renderer = (EditorPropertyRenderer) constructor.Invoke(TypeUtils.EMPTY_PARAMETERS);
            
                // Maintain
                this.customRenderers[this.rendererTypeName] = renderer;

                return Option<EditorPropertyRenderer>.Some(renderer);
            }

            public Option<EditorPropertyRenderer> OnNone() {
                return Option<EditorPropertyRenderer>.NONE;
            }
        }

        private Option<EditorPropertyRenderer> ResolveRenderer(string rendererTypeName) {
            Option<EditorPropertyRenderer> renderer = this.customRenderers.Find(rendererTypeName);
            if (renderer.IsSome) {
                // Already exists
                return renderer;
            }
            
            // At this point, renderer does not exist yet
            // Let's make one
            Option<Type> foundType = TypeIdentifier.GetType(rendererTypeName);
            Assertion.IsSome(foundType, rendererTypeName);

            return foundType.MatchExplicit<CreateEditorPropertyRendererMatcher, Option<EditorPropertyRenderer>>(
                new CreateEditorPropertyRendererMatcher(rendererTypeName, this.customRenderers));
        }

        private static int AscendingNameComparison(PropertyInfo a, PropertyInfo b) {
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        private List<PropertyInfo> ResolveGroupedList(string groupName) {
            if (this.groupMap.TryGetValue(groupName, out List<PropertyInfo> list)) {
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
            string? value = property.GetGetMethod().Invoke(instance, null) as string;
            value = string.IsNullOrEmpty(value) ? "" : value; // Prevent null

            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = EditorGUILayout.TextField(value, GUILayout.Width(300)).Trim();
            GUILayout.EndHorizontal();

            // Set the value back
            property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        private static void RenderByte(PropertyInfo property, object instance) {
            byte value = (byte)property.GetGetMethod().Invoke(instance, null);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", GUILayout.Width(150));
            value = (byte)EditorGUILayout.IntField(value, GUILayout.Width(150));
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

        private static void RenderFloat2(PropertyInfo property, object instance) {
            float2 value = (float2)property.GetGetMethod().Invoke(instance, null);

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

        private static void RenderStringList(PropertyInfo property, object instance) {
            GUILayout.Label(property.Name);
            
            List<string> list = (List<string>)property.GetGetMethod().Invoke(instance, null);
            if (list == null) {
                GUILayout.Label("(list is null)");
                return;
            }

            EditorRenderUtils.Render(property.Name, list);
        }
    }
}
