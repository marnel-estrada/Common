using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common {
    public class RangeWithStringIdRenderer {
        private readonly SimpleList<RangeDataWithSingleString> rawRangeDataRemovalList = new SimpleList<RangeDataWithSingleString>();

        // Placeholders when making a new entry
        private string newEntryName = string.Empty;
        private int newMinCount;
        private int newMaxCount;

        /// <summary>
        /// Generic renderer for a <see cref="StringCountData"/>
        /// Returns whether or not there changes on the list
        /// </summary>
        /// <param name="title"></param>
        /// <param name="entryList"></param>
        /// <param name="minLabel"></param>
        /// <param name="maxLabel"></param>
        /// <param name="stringLabel"></param>
        public bool Render(string title, List<RangeDataWithSingleString> entryList, string minLabel = "Min:", string maxLabel = "Max:", string stringLabel = "Entry Name:") {
            bool changed = false;

            // New entry
            GUI.backgroundColor = ColorUtils.YELLOW;
            GUILayout.Box(title, GUILayout.Width(400));
            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.Label(minLabel, GUILayout.Width(40));
            this.newMinCount = EditorGUILayout.IntField(this.newMinCount, GUILayout.Width(30));

            GUILayout.Space(5);

            GUILayout.Label(maxLabel, GUILayout.Width(40));
            this.newMaxCount = EditorGUILayout.IntField(this.newMaxCount, GUILayout.Width(30));

            GUILayout.Space(5);

            GUILayout.Label(stringLabel, GUILayout.Width(70));
            this.newEntryName = EditorGUILayout.TextField(this.newEntryName, GUILayout.Width(120));

            GUILayout.Space(5);

            if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(15))) {
                if (AddNewEntry(entryList)) {
                    // Mark the data as dirty to refresh the editor
                    return true;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Existing entries
            for (int i = 0; i < entryList.Count; ++i) {
                RangeDataWithSingleString entry = RenderEntry(entryList[i]);
                entryList[i] = entry;
            }

            changed = changed || this.rawRangeDataRemovalList.Count > 0;

            for (int i = 0; i < this.rawRangeDataRemovalList.Count; ++i) {
                entryList.Remove(this.rawRangeDataRemovalList[i]);
            }

            this.rawRangeDataRemovalList.Clear();

            return changed;
        }

        private RangeDataWithSingleString RenderEntry(RangeDataWithSingleString entry) {
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
                this.rawRangeDataRemovalList.Add(entry);
                return entry;
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Label("Min Count:", GUILayout.Width(70));
            entry.min = EditorGUILayout.FloatField(entry.min, GUILayout.Width(50));

            GUILayout.Label("Max Count:", GUILayout.Width(70));
            entry.max = EditorGUILayout.FloatField(entry.max, GUILayout.Width(50));

            GUILayout.Label("Entry Name:", GUILayout.Width(90));
            entry.stringId = EditorGUILayout.TextField(entry.stringId, GUILayout.Width(150));
            GUILayout.Space(5);

            GUILayout.EndHorizontal();

            return entry;
        }

        /// <summary>
        /// Performs existence checking for the new entry
        /// </summary>
        /// <param name="entryList"></param>
        /// <returns>A boolean to mark whether the list has changed or not</returns>
        private bool AddNewEntry(ICollection<RangeDataWithSingleString> entryList) {
            if (string.IsNullOrEmpty(this.newEntryName)) {
                EditorUtility.DisplayDialog("Add Entry", "Can't add. Entry name is empty.", "OK");
                return false;
            }

            entryList.Add(new RangeDataWithSingleString {
                min = this.newMinCount,
                max = this.newMaxCount,
                stringId = this.newEntryName
            });

            return true;
        }
    }
}