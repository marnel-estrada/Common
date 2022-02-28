using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Common {
    public class RangeWithStringListRenderer {
        private readonly SimpleList<RangeDataWithStringList> rawRangeDataRemovalList = new SimpleList<RangeDataWithStringList>();

        // Placeholders when making a new entry
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
        public bool Render(string title, List<RangeDataWithStringList> entryList, string minLabel = "Min:", string maxLabel = "Max:") {
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
                RangeDataWithStringList entry = RenderEntry(entryList[i], ref changed);
                entryList[i] = entry;
            }

            changed = changed || this.rawRangeDataRemovalList.Count > 0;

            for (int i = 0; i < this.rawRangeDataRemovalList.Count; ++i) {
                entryList.Remove(this.rawRangeDataRemovalList[i]);
            }

            this.rawRangeDataRemovalList.Clear();

            return changed;
        }

        private RangeDataWithStringList RenderEntry(RangeDataWithStringList entry, ref bool changed) {
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(15))) {
                if (EditorUtility.DisplayDialogComplex("Remove Entry",
                    "Are you sure you want to remove this data?",
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
            entry.min = EditorGUILayout.IntField(entry.min, GUILayout.Width(50));

            GUILayout.Label("Max Count:", GUILayout.Width(70));
            entry.max = EditorGUILayout.IntField(entry.max, GUILayout.Width(50));

            GUILayout.BeginVertical();
            if (entry.stringList != null && EditorRenderUtils.Render("String Ids:", entry.stringList)) {
                changed = true;
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            return entry;
        }

        /// <summary>
        /// Performs existence checking for the new entry
        /// </summary>
        /// <param name="entryList"></param>
        /// <returns>A boolean to mark whether the list has changed or not</returns>
        private bool AddNewEntry(ICollection<RangeDataWithStringList> entryList) {
            entryList.Add(new RangeDataWithStringList {
                min = this.newMinCount,
                max = this.newMaxCount,
                stringList = new List<string>()
            });

            return true;
        }
    }
}