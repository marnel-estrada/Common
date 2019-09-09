using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class TypeGroupView {
        private readonly string[] labelList;

        private readonly List<Type> typeList;

        private bool expanded;

        private readonly Action<TypeGroupView> onSelect;

        private readonly Action<Type> onSelectionChange;

        private int selected = -1;

        private readonly GUIStyle style;

        /**
		 * Constructor
		 */
        public TypeGroupView(string name, List<Type> typeList, GUIStyle style, Action<Type> onSelectionChange,
            Action<TypeGroupView> onSelect) {
            this.Name = name;
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

        public string Name { get; }

        /**
		 * Render routines
		 */
        public void Render() {
            GUILayout.BeginVertical();

            this.expanded = EditorGUILayout.Foldout(this.expanded, this.Name);

            if (this.expanded) {
                RenderItems();
            }

            GUILayout.EndVertical();
        }

        private void RenderItems() {
            for (int i = 0; i < this.typeList.Count; ++i) {
                Type type = this.typeList[i];

                Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(type.Name), this.style);
                bool hover = elementRect.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseDown) {
                    this.selected = i;
                    Event.current.Use();

                    // dispatch signal that selection has changed
                    this.onSelectionChange(type); // Invoke

                    this.onSelect(this); // invoke
                } else if (Event.current.type == EventType.Repaint) {
                    this.style.Draw(elementRect, type.Name, hover, false, i == this.selected, i == this.selected);
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