using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Common {
    /// <summary>
    /// An editor renderer that renders
    /// </summary>
    public class TypeSelectionRenderer {

        private readonly Dictionary<string, List<Type>> typeMap = new Dictionary<string, List<Type>>();
        private readonly List<TypeGroupView> groupViewList = new List<TypeGroupView>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="style"></param>
        public TypeSelectionRenderer(Type parentType, GUIStyle style, Action<Type> onSelectionChange) {
            List<Type> allTypes = new List<Type>();

            // Add all assemblies from current domain
            Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in currentAssemblies) {
                // We did it this way because some assemblies causes error when GetTypes() is invoked
                try {
                    allTypes.AddRange(assembly.GetTypes());
                } catch(Exception e) {
                    Debug.Log($"Assembly {assembly.FullName} caused an error.");
                    Debug.Log(e.Message);
                }
            }

            Init(parentType, allTypes.ToArray(), style, onSelectionChange);
        }

        /// <summary>
        /// Constructor with specific types
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="types"></param>
        /// <param name="style"></param>
        /// <param name="onSelectionChange"></param>
        public TypeSelectionRenderer(Type parentType, Type[] types, GUIStyle style, Action<Type> onSelectionChange) {
            Init(parentType, types, style, onSelectionChange);
        }

        private void Init(Type parentType, Type[] types, GUIStyle style, Action<Type> onSelectionChange) {
            // populate action types
            foreach (Type type in types) {
                if (type.IsSubclassOf(parentType) && !type.IsAbstract) {
                    AddToMap(type);
                }
            }

            // populate groupViewList
            foreach (KeyValuePair<string, List<Type>> entry in this.typeMap) {
                this.groupViewList.Add(new TypeGroupView(entry.Key, entry.Value, style, onSelectionChange, OnSelect));
            }

            this.groupViewList.Sort(delegate(TypeGroupView a, TypeGroupView b) {
                return a.Name.CompareTo(b.Name);
            });
        }

        private void OnSelect(TypeGroupView selectedView) {
            // clear other and retain only the selected action (remove the highlight)
            foreach (TypeGroupView view in this.groupViewList) {
                if (view != selectedView) {
                    view.ClearSelection();
                }
            }
        }

        private void AddToMap(Type type) {
            // identify group
            string groupName = GetGroupName(type);

            if (this.typeMap.ContainsKey(groupName)) {
                this.typeMap[groupName].Add(type);
            } else {
                // list is not yet existing, we create a new one
                this.typeMap[groupName] = new List<Type>();
                this.typeMap[groupName].Add(type);
            }
        }

        private static string GetGroupName(Type type) {
            Attribute[] attributes = Attribute.GetCustomAttributes(type);
            foreach (Attribute attr in attributes) {
                if (attr is Group) {
                    return ((Group)attr).Name;
                }
            }

            // the type may have no ActionGroup attribute
            return "Ungrouped";
        }

        private Vector2 scrollPos = new Vector2();

        /// <summary>
        /// Performs the rendering in editor
        /// </summary>
        public void Render() {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            foreach (TypeGroupView view in this.groupViewList) {
                view.Render();
            }
            GUILayout.EndScrollView();
        }

    }
}
