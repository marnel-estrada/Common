using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Common;
using Common.Signal;

namespace Common {
    public class TypeGroupView {

        private readonly string name; // the group name
        private readonly List<Type> typeList;
        private readonly string[] labelList;

        private int selected = -1;

        private GUIStyle style;

        private Action<Type> onSelectionChange;

        private Action<TypeGroupView> onSelect;

        /**
		 * Constructor
		 */
        public TypeGroupView(string name, List<Type> typeList, GUIStyle style, Action<Type> onSelectionChange, Action<TypeGroupView> onSelect) {
            this.name = name;
            this.typeList = typeList;
            this.style = style;
            this.onSelectionChange = onSelectionChange;
            this.onSelect = onSelect;

            // Sort
            this.typeList.Sort(delegate(Type a, Type b) {
                return a.Name.CompareTo(b.Name);
            });
            
            // populate from typeList
            this.labelList = new string[typeList.Count];
            for (int i = 0; i < typeList.Count; ++i) {
                this.labelList[i] = typeList[i].Name;
            }
        }

        private bool expanded = false;

        public string Name {
            get {
                return name;
            }
        }

        /**
		 * Render routines
		 */
        public void Render() {
            GUILayout.BeginVertical();

            this.expanded = EditorGUILayout.Foldout(this.expanded, this.name);

            if (this.expanded) {
                RenderItems();
            }

            GUILayout.EndVertical();
        }

        private void RenderItems() {
            for (int i = 0; i < typeList.Count; ++i) {
                Type type = typeList[i];

                Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(type.Name), this.style);
                bool hover = elementRect.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseDown) {
                    selected = i;
                    Event.current.Use();

                    // dispatch signal that selection has changed
                    this.onSelectionChange(type); // Invoke

                    this.onSelect(this); // invoke
                } else if (Event.current.type == EventType.Repaint) {
                    this.style.Draw(elementRect, type.Name, hover, false, i == selected, i == selected);
                }
            }
        }

        /**
		 * Clears the currently selected action.
		 */
        public void ClearSelection() {
            this.selected = -1;
        }

    }
}
