using System;
using System.Collections.Generic;

using Common;

using UnityEditor;

using UnityEngine;

using EditorGUILayout = UnityEditor.Experimental.Networking.PlayerConnection.EditorGUILayout;

namespace GameEvent {
    public class EventsBrowserWindow : EditorWindow {
        private EditorWindow parent;
        
        private DataPool<EventData> pool;
        private EventData eventData;
        private OptionData option;
        
        private readonly List<EventData> filteredEvents = new List<EventData>(100);

        public void Init(EditorWindow parent, DataPool<EventData> pool, EventData eventData, OptionData option) {
            this.parent = parent;
            
            this.pool = pool;
            this.eventData = eventData;
            this.option = option;
            
            // We call this so that filteredEvents would be populated with all events by default
            ApplyFilter();
        }
        
        private Vector2 scrollPosition = new Vector2();

        private void OnGUI() {
            GUILayout.BeginVertical();
            
            GUILayout.Label("Events Browser", EditorStyles.boldLabel);
            GUILayout.Space(10);

            RenderFilter();
            
            GUILayout.Space(10);
            
            RenderEvents();
            
            GUILayout.FlexibleSpace();

            GUILayout.Space(5);

            if (GUILayout.Button("Select", GUILayout.Width(100))) {
                if (this.selectedIndex >= 0) {
                    // Set only if there was indeed a selected event   
                    this.option.ChildEventId = this.filteredEvents[this.selectedIndex].IntId;
                    EditorUtility.SetDirty(this.pool);
                    this.parent.Repaint();
                    Close();
                } else {
                    EditorUtility.DisplayDialog("Events Browser", "There is no selected event.", "OK");
                }
            }
            
            GUILayout.EndVertical();
        }

        private string filterText = "";

        private void RenderFilter() {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ", GUILayout.Width(100));
            string newFilterText = GUILayout.TextField(this.filterText, GUILayout.Width(150));

            if (!newFilterText.EqualsFast(this.filterText)) {
                // Search only if new filter text is not the same as the previous one
                this.filterText = newFilterText;
                ApplyFilter();
            }
            
            GUILayout.EndHorizontal();
        }

        private void ApplyFilter() {
            this.filteredEvents.Clear();
            
            if (string.IsNullOrEmpty(this.filterText)) {
                // No filter. Add all.
                AddAll();

                return;
            }
            
            // At this point, it means there's a filter text
            // We search
            AddFilteredEvents();
        }

        private void AddFilteredEvents() {
            foreach (EventData item in this.pool.GetAll()) {
                if (item == this.eventData) {
                    // Do not include the parent event of the option to avoid circular dependency
                    continue;
                }
                
                if (item.NameId.Contains(this.filterText) || item.DescriptionId.Contains(this.filterText)) {
                    this.filteredEvents.Add(item);
                }
            }
        }

        private void AddAll() {
            foreach (EventData item in this.pool.GetAll()) {
                if (item == this.eventData) {
                    // Do not include the parent event of the option to avoid circular dependency
                    continue;
                }
                
                this.filteredEvents.Add(item);
            }
        }

        private int selectedIndex = -1;

        private void RenderEvents() {
            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);
            GUILayout.Label("Filtered Events:");
            
            GUIStyle style = this.pool.Skin.customStyles[0];
            
            for (int i = 0; i < this.filteredEvents.Count; ++i) {
                EventData item = this.filteredEvents[i];

                Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(item.NameId), style);
                bool hover = elementRect.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseDown) {
                    this.selectedIndex = i;
                    Event.current.Use();
                } else if (Event.current.type == EventType.Repaint) {
                    style.Draw(elementRect, item.NameId, hover, false, i == this.selectedIndex, i == this.selectedIndex);
                }
            }
            
            GUILayout.EndScrollView();
        }
        
        private void Update() {
            // close the window if editor is compiling
            if (EditorApplication.isCompiling) {
                Close();
            }
        }
        
        private void OnLostFocus() {
            Close();
        }
    }
}