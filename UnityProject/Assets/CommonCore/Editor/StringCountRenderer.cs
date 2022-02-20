using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common {
    public class StringCountRenderer {
        private readonly SimpleList<StringCountData> stringCountRemovalList = new SimpleList<StringCountData>();

        // Placeholders when making a new entry
        private string newEntryName = string.Empty;
        private int newEntryCount;

        /// <summary>
        /// Generic renderer for a <see cref="StringCountData"/>
        /// Returns whether or not there changes on the list
        /// </summary>
        /// <param name="title"></param>
        /// <param name="entryList"></param>
        /// <param name="countLabel"></param>
        public bool Render(string title, List<StringCountData> entryList, string countLabel = "Min Count:") {
            bool changed = false;

            // New entry
            GUILayout.Label(title);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Entry Name:", GUILayout.Width(90));
            this.newEntryName = EditorGUILayout.TextField(this.newEntryName, GUILayout.Width(150));

            GUILayout.Space(5);

            GUILayout.Label(countLabel, GUILayout.Width(70));
            this.newEntryCount = EditorGUILayout.IntField(this.newEntryCount, GUILayout.Width(50));

            if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(15))) {
                if (AddNewEntry(entryList, this.newEntryName, this.newEntryCount)) {
                    // Mark the data as dirty to refresh the editor
                    return true;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Existing entries
            for (int i = 0; i < entryList.Count; ++i) {
                StringCountData entry = RenderEntry(entryList[i]);
                entryList[i] = entry;
            }

            changed = changed || this.stringCountRemovalList.Count > 0;

            for (int i = 0; i < this.stringCountRemovalList.Count; ++i) {
                entryList.Remove(this.stringCountRemovalList[i]);
            }

            this.stringCountRemovalList.Clear();

            return changed;
        }

        private StringCountData RenderEntry(StringCountData entry) {
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(15))) {
                if (EditorUtility.DisplayDialogComplex("Remove Entry",
                    "Are you sure you want to remove '{0}'?".FormatWith(entry.stringId),
                    "Yes", "No", "Cancel") != 0) {
                    // No or Cancelled
                    return entry;
                }

                // Add current item to the removal list and process later
                this.stringCountRemovalList.Add(entry);
                return entry;
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Label("Entry Name:", GUILayout.Width(90));
            entry.stringId = EditorGUILayout.TextField(entry.stringId, GUILayout.Width(150));
            GUILayout.Space(5);

            GUILayout.Label("Min Count:", GUILayout.Width(70));
            entry.count = EditorGUILayout.IntField(entry.count, GUILayout.Width(50));

            GUILayout.EndHorizontal();

            return entry;
        }

        /// <summary>
        /// Performs existence checking for the new entry
        /// </summary>
        /// <param name="entryList"></param>
        /// <param name="newEntryName"></param>
        /// <param name="newEntryCount"></param>
        /// <returns>A boolean to mark whether the list has changed or not</returns>
        private static bool AddNewEntry(List<StringCountData> entryList, string newEntryName, int newEntryCount) {
            if (string.IsNullOrEmpty(newEntryName)) {
                EditorUtility.DisplayDialog("Add Entry", "Can't add. Entry name is empty.", "OK");
                return false;
            }

            if (newEntryCount <= 0) {
                EditorUtility.DisplayDialog("Add Entry", "Invalid Min Count", "OK");
                return false;
            }

            // Check if it already exists
            for (int i = 0; i < entryList.Count; ++i) {
                if (entryList[i].stringId.EqualsFast(newEntryName)) {
                    EditorUtility.DisplayDialog(
                        "Add Entry", "Entry '{0}' already exists."
                            .FormatWith(newEntryName), "OK");
                    return false;
                }
            }

            StringCountData newEntry = new StringCountData(newEntryName, newEntryCount);
            entryList.Add(newEntry);

            return true;
        }
    }
}