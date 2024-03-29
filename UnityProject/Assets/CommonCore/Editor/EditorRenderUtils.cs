﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Common {
    public static class EditorRenderUtils {
        /// <summary>
        /// Renders a dropdown list or popup
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueSet"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string Dropdown(string value, PopupValueSet valueSet, int width) {
            int index = valueSet.ResolveIndex(value);
            if (index < 0) {
                // current value is not found in the value set
                // we use the first entry instead
                index = 0;
            }

            index = EditorGUILayout.Popup(index, valueSet.DisplayList, GUILayout.Width(width));
            return valueSet.GetValue(index);
        }

        private static string NEW_ENTRY = "";
        private static readonly SimpleList<string> REMOVAL_LIST = new SimpleList<string>();

        /// <summary>
        /// Generic renderer for a string list
        /// Returns whether or not there changes on the list
        /// </summary>
        /// <param name="title"></param>
        /// <param name="stringList"></param>
        public static bool Render(string title, List<string> stringList) {
            bool changed = false;

            // New entry
            GUILayout.BeginHorizontal();
            GUILayout.Label("New: ", GUILayout.Width(40));
            NEW_ENTRY = GUILayout.TextField(NEW_ENTRY, GUILayout.Width(200));
            if (GUILayout.Button("Add", GUILayout.Width(50))) {
                if (!string.IsNullOrEmpty(NEW_ENTRY)) {
                    stringList.Add(NEW_ENTRY);
                    changed = true;
                    NEW_ENTRY = string.Empty;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Existing entries
            for (int i = 0; i < stringList.Count; ++i) {
                GUILayout.BeginHorizontal();

                // Close button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    if (EditorUtility.DisplayDialogComplex(title, $"Are you sure you want to remove {stringList[i]}?", "Yes", "No", "Cancel") == 0) {
                        REMOVAL_LIST.Add(stringList[i]);
                    }
                }

                GUI.backgroundColor = Color.white;

                string current = stringList[i];
                string field = GUILayout.TextField(current, GUILayout.Width(200));
                if (!current.EqualsFast(field)) {
                    // There was a rename
                    stringList[i] = field;
                    changed = true;
                }

                GUILayout.EndHorizontal();
            }

            // Apply removal list
            changed = changed || REMOVAL_LIST.Count > 0;
            for (int i = 0; i < REMOVAL_LIST.Count; ++i) {
                stringList.Remove(REMOVAL_LIST[i]);
            }

            REMOVAL_LIST.Clear();

            return changed;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2f;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}