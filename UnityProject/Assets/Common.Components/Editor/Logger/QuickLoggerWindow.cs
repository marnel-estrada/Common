using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Common.Logger;
using System;

namespace Common {
    public class QuickLoggerWindow : EditorWindow {

        private bool playing = false;

        // treat as constructor
        void OnEnable() {
            UpdateFilteredLogs();
        }

        void OnGUI() {
            try {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Quick Logger", EditorStyles.largeLabel);

                GUILayout.Space(10);

                if (!EditorApplication.isPlaying) {
                    EditorGUILayout.HelpBox("Logs are only shown on play mode.", MessageType.Info);
                    return;
                }

                // normal rendering here
                RenderLogs();
            } finally {
                EditorGUILayout.EndVertical();
            }
        }

        private Vector2 scrollPos = new Vector2(0, 1);

        private string previousFilterText = "";
        private string filterText = "";
        private readonly List<Log> filteredLogs = new List<Log>();

        private void RenderLogs() {
            RenderSearch();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUILayout.ExpandWidth(true));

            for(int i = 0; i < this.filteredLogs.Count; ++i) {
                EditorGUILayout.SelectableLabel(this.filteredLogs[i].Message, GUILayout.Height(20));
            }

            GUILayout.EndScrollView();
        }

        private void RenderSearch() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(70));
            this.filterText = EditorGUILayout.TextField(this.filterText, GUILayout.Width(200));

            if(!this.filterText.Equals(this.previousFilterText)) {
                // this means that a new search has been specified
                // we updated the filtered list
                UpdateFilteredLogs();
                this.previousFilterText = this.filterText;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateFilteredLogs() {
            this.filteredLogs.Clear();

            if(string.IsNullOrEmpty(this.filterText)) {
                // filter text is empty
                // add all logs
                for(int i = 0; i < QuickLogger.Count; ++i) {
                    this.filteredLogs.Add(QuickLogger.GetAt(i));
                }
                return;
            }

            // at this point, a filter text is specified
            // we do filtering
            for(int i = 0; i < QuickLogger.Count; ++i) {
                Log log = QuickLogger.GetAt(i);
                if(log.Message.CaseInsensitiveContains(this.filterText)) {
                    this.filteredLogs.Add(log);
                }
            }
        }

        private int previousCount = int.MinValue;

        void Update() {
            if(EditorApplication.isPlaying) {
                // we only invoke this if playing so that in won't create QuickLogger objects
                CheckNewLogs();
            }

            Repaint();
        }

        private void CheckNewLogs() {
            if(this.previousCount != QuickLogger.Count) {
                // this means that new logs came in
                // we update the view
                UpdateFilteredLogs();
                this.previousCount = QuickLogger.Count;
            }
        }

        [MenuItem("Common/QuickLogger #&q")]
        public static void OpenWindow() {
            GetWindow<QuickLoggerWindow>("Quick Logger");
        }

    }
}
