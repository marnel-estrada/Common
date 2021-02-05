using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace DTCompileTimeTracker {
    [InitializeOnLoad]
    public static class CompileTimeTracker {
        private const string K_COMPILE_TIME_TRACKER_KEY = "CompileTimeTracker::_data";

        private static readonly AudioClip? COMPILATION_COMPLETED_CLIP;
        private static CompileTimeTrackerData? DATA;

        static CompileTimeTracker() {
            EditorApplicationCompilationUtil.StartedCompiling += HandleEditorStartedCompiling;
            EditorApplicationCompilationUtil.FinishedCompiling += HandleEditorFinishedCompiling;

            string[] foundResults = AssetDatabase.FindAssets("t:AudioClip CompileFinished");
            if (foundResults.Length > 0) {
                // Get only the first one
                string guid = foundResults[0];
                COMPILATION_COMPLETED_CLIP =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.IsNotNull(COMPILATION_COMPLETED_CLIP);
            }
        }

        private static CompileTimeTrackerData Data {
            get {
                if (DATA == null) {
                    DATA = new CompileTimeTrackerData(K_COMPILE_TIME_TRACKER_KEY);
                }

                return DATA;
            }
        }

        private static int StoredErrorCount {
            get {
                return EditorPrefs.GetInt("CompileTimeTracker::StoredErrorCount");
            }
            set {
                EditorPrefs.SetInt("CompileTimeTracker::StoredErrorCount", value);
            }
        }

        public static event Action<CompileTimeKeyframe> KeyframeAdded = delegate {
        };

        public static IList<CompileTimeKeyframe> GetCompileTimeHistory() {
            return Data.GetCompileTimeHistory();
        }

        private static void HandleEditorStartedCompiling() {
            Data.StartTime = TrackingUtil.GetMilliseconds();

            UnityConsoleCountsByType countsByType = UnityEditorConsoleUtil.GetCountsByType();
            StoredErrorCount = countsByType.errorCount;
        }

        private static void HandleEditorFinishedCompiling() {
            int elapsedTime = TrackingUtil.GetMilliseconds() - Data.StartTime;

            UnityConsoleCountsByType countsByType = UnityEditorConsoleUtil.GetCountsByType();
            bool hasErrors = countsByType.errorCount - StoredErrorCount > 0;

            CompileTimeKeyframe keyframe = new CompileTimeKeyframe(elapsedTime, hasErrors);
            Data.AddCompileTimeKeyframe(keyframe);
            KeyframeAdded.Invoke(keyframe);

            if (COMPILATION_COMPLETED_CLIP != null) {
                PlayClip(COMPILATION_COMPLETED_CLIP);
            }
        }

        private static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            const string audioPlayerPreviewName = "PlayPreviewClip";
            MethodInfo? method = audioUtilClass.GetMethod(audioPlayerPreviewName, BindingFlags.Static | BindingFlags.Public, null,
                new[] {
                    typeof(AudioClip), typeof(int), typeof(bool)
                }, null);

            if (method == null) {
                Debug.LogError($"{audioPlayerPreviewName} can't be found");
            } else {
                // The method exists
                method.Invoke(null, new object[] {
                    clip, startSample, loop
                });
            }
        }
    }
}