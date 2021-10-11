using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common {
    public class TypeGroupView {
        private readonly List<Type> typeList;

        private bool expanded;
        private bool wasExpandedBeforeFiltering;

        private readonly Action<TypeGroupView> onSelect;

        private readonly Action<Type> onSelectionChange;

        private int selected = -1;

        private readonly GUIStyle style;
        private const string EMPTY = "";

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
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        public string Name { get; }

        /*
		 * Render routines
		 */
        public void Render(string filter = EMPTY) {
            GUILayout.BeginVertical();

            bool isSearching = !string.IsNullOrEmpty(filter);

            if (isSearching) {
                this.expanded = HasMatchedFilter(filter);

                // Only show the foldout when a match has been found 
                if (this.expanded) {
                    this.expanded = EditorGUILayout.Foldout(this.expanded, this.Name);
                }
            } else {
                this.expanded = this.wasExpandedBeforeFiltering;
                this.expanded = EditorGUILayout.Foldout(this.expanded, this.Name);
                this.wasExpandedBeforeFiltering = this.expanded;
            }

            if (this.expanded) {
                RenderItems();
            }

            GUILayout.EndVertical();
        }

        private bool HasMatchedFilter(string filter = EMPTY) {
            // Check if a type under this group matches the filter
            for (int i = 0; i < this.typeList.Count; ++i) {
                Type type = this.typeList[i];
                string typeName = type.Name;

                if (typeName.ToLower().Contains(filter.ToLower())) {
                    return true;
                }
            }

            // Lastly, check the group's name
            return this.Name.ToLower().Contains(filter.ToLower());
        }

        private void RenderItems(string filter = EMPTY) {
            for (int i = 0; i < this.typeList.Count; ++i) {
                Type type = this.typeList[i];
                string typeName = type.Name;

                if (!typeName.ToLower().Contains(filter.ToLower())) {
                    continue;
                }

                Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(typeName), this.style);
                bool hover = elementRect.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseDown) {
                    this.selected = i;
                    Event.current.Use();

                    // dispatch signal that selection has changed
                    this.onSelectionChange(type); // Invoke

                    this.onSelect(this); // invoke
                } else if (Event.current.type == EventType.Repaint) {
                    this.style.Draw(elementRect, typeName, hover, false, i == this.selected, i == this.selected);
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