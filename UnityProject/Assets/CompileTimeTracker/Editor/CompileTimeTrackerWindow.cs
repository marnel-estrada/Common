using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace DTCompileTimeTracker {
    [InitializeOnLoad]
    public class CompileTimeTrackerWindow : EditorWindow {

        // PRAGMA MARK - Internal
        private Vector2 scrollPosition;

        // PRAGMA MARK - Static
        static CompileTimeTrackerWindow() {
            CompileTimeTracker.KeyframeAdded += LogCompileTimeKeyframe;
        }

        private static bool ShowErrors {
            get {
                return EditorPrefs.GetBool("CompileTimeTrackerWindow.ShowErrors");
            }
            set {
                EditorPrefs.SetBool("CompileTimeTrackerWindow.ShowErrors", value);
            }
        }

        private static bool OnlyToday {
            get {
                return EditorPrefs.GetBool("CompileTimeTrackerWindow.OnlyToday");
            }
            set {
                EditorPrefs.SetBool("CompileTimeTrackerWindow.OnlyToday", value);
            }
        }

        private static bool OnlyYesterday {
            get {
                return EditorPrefs.GetBool("CompileTimeTrackerWindow.OnlyYesterday");
            }
            set {
                EditorPrefs.SetBool("CompileTimeTrackerWindow.OnlyYesterday", value);
            }
        }

        private static bool LogToConsole {
            get {
                return EditorPrefs.GetBool("CompileTimeTrackerWindow.LogToConsole", true);
            }
            set {
                EditorPrefs.SetBool("CompileTimeTrackerWindow.LogToConsole", value);
            }
        }

        [MenuItem("Window/Compile Time Tracker Window")]
        public static void Open() {
            GetWindow<CompileTimeTrackerWindow>(false, "Compile Timer Tracker", true);
        }

        private static void LogCompileTimeKeyframe(CompileTimeKeyframe keyframe) {
            bool dontLogToConsole = !LogToConsole;
            if (dontLogToConsole) {
                return;
            }

            string compilationFinishedLog =
                "Compilation Finished: " + TrackingUtil.FormatMSTime(keyframe.elapsedCompileTimeInMS);
            if (keyframe.hadErrors) {
                compilationFinishedLog += " (error)";
            }

            Debug.Log(compilationFinishedLog);
        }

        private void OnGUI() {
            Rect screenRect = this.position;
            int totalCompileTimeInMs = 0;

            // show filters
            EditorGUILayout.BeginHorizontal(GUILayout.Height(20.0f));
            EditorGUILayout.Space();
            float toggleRectWidth = screenRect.width / 4.0f;
            Rect toggleRect = new Rect(0.0f, 0.0f, toggleRectWidth, 20.0f);

            // Psuedo enum logic here
            if (OnlyToday && OnlyYesterday) {
                OnlyYesterday = false;
            }

            if (!OnlyToday && !OnlyYesterday) {
                OnlyToday = true;
            }

            bool newOnlyToday = GUI.Toggle(toggleRect, OnlyToday, "Today", "Button");
            if (newOnlyToday != OnlyToday) {
                OnlyToday = newOnlyToday;
                OnlyYesterday = !newOnlyToday;
            }

            toggleRect.position = toggleRect.position.AddX(toggleRectWidth);
            bool newOnlyYesterday = GUI.Toggle(toggleRect, OnlyYesterday, "Yesterday", "Button");
            if (newOnlyYesterday != OnlyYesterday) {
                OnlyYesterday = newOnlyYesterday;
                OnlyToday = !newOnlyYesterday;
            }
            // End psuedo enum logic

            toggleRect.position = toggleRect.position.AddX(2.0f * toggleRectWidth);
            ShowErrors = GUI.Toggle(toggleRect, ShowErrors, "Errors", "Button");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Height(20.0f));
            LogToConsole = EditorGUILayout.Toggle("Log Compile Time", LogToConsole);
            EditorGUILayout.EndHorizontal();

            this.scrollPosition =
                EditorGUILayout.BeginScrollView(this.scrollPosition, GUILayout.Height(screenRect.height - 60.0f));
            foreach (CompileTimeKeyframe keyframe in GetFilteredKeyframes()) {
                string compileText = $"({keyframe.Date:hh:mm tt}): ";
                compileText += TrackingUtil.FormatMSTime(keyframe.elapsedCompileTimeInMS);
                if (keyframe.hadErrors) {
                    compileText += " (error)";
                }

                GUILayout.Label(compileText);

                totalCompileTimeInMs += keyframe.elapsedCompileTimeInMS;
            }

            EditorGUILayout.EndScrollView();

            string statusBarText = "Total compile time: " + TrackingUtil.FormatMSTime(totalCompileTimeInMs);
            if (EditorApplication.isCompiling) {
                statusBarText = "Compiling.. || " + statusBarText;
            }

            GUILayout.Label(statusBarText);
        }

        private void OnEnable() {
            EditorApplicationCompilationUtil.StartedCompiling += HandleEditorStartedCompiling;
            CompileTimeTracker.KeyframeAdded += HandleCompileTimeKeyframeAdded;
        }

        private void OnDisable() {
            EditorApplicationCompilationUtil.StartedCompiling -= HandleEditorStartedCompiling;
            CompileTimeTracker.KeyframeAdded -= HandleCompileTimeKeyframeAdded;
        }

        private IEnumerable<CompileTimeKeyframe> GetFilteredKeyframes() {
            IEnumerable<CompileTimeKeyframe> filteredKeyframes = CompileTimeTracker.GetCompileTimeHistory();
            if (!ShowErrors) {
                filteredKeyframes = filteredKeyframes.Where(keyframe => !keyframe.hadErrors);
            }

            if (OnlyToday) {
                filteredKeyframes =
                    filteredKeyframes.Where(keyframe => DateTimeUtil.SameDay(keyframe.Date, DateTime.Now));
            } else if (OnlyYesterday) {
                filteredKeyframes = filteredKeyframes.Where(keyframe =>
                    DateTimeUtil.SameDay(keyframe.Date, DateTime.Now.AddDays(-1)));
            }

            return filteredKeyframes;
        }

        private void HandleEditorStartedCompiling() {
            Repaint();
        }

        private void HandleCompileTimeKeyframeAdded(CompileTimeKeyframe keyframe) {
            Repaint();
        }
    }
}