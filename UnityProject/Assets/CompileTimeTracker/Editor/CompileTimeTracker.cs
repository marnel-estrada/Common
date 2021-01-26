using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using UnityEngine.Assertions;

namespace DTCompileTimeTracker {
    [InitializeOnLoad]
    public static class CompileTimeTracker {
        private const string kCompileTimeTrackerKey = "CompileTimeTracker::_data";

        private static readonly AudioClip COMPILATION_COMPLETED_CLIP;
        private static CompileTimeTrackerData _data;

        static CompileTimeTracker() {
            EditorApplicationCompilationUtil.StartedCompiling += HandleEditorStartedCompiling;
            EditorApplicationCompilationUtil.FinishedCompiling += HandleEditorFinishedCompiling;

            string guid = AssetDatabase.FindAssets("t:AudioClip CompileFinished")[0];
            COMPILATION_COMPLETED_CLIP = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
            Assert.IsNotNull(COMPILATION_COMPLETED_CLIP);
        }

        private static CompileTimeTrackerData _Data {
            get {
                if (_data == null) {
                    _data = new CompileTimeTrackerData(kCompileTimeTrackerKey);
                }

                return _data;
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
            return _Data.GetCompileTimeHistory();
        }

        private static void HandleEditorStartedCompiling() {
            _Data.StartTime = TrackingUtil.GetMilliseconds();

            UnityConsoleCountsByType countsByType = UnityEditorConsoleUtil.GetCountsByType();
            StoredErrorCount = countsByType.errorCount;
        }

        private static void HandleEditorFinishedCompiling() {
            int elapsedTime = TrackingUtil.GetMilliseconds() - _Data.StartTime;

            UnityConsoleCountsByType countsByType = UnityEditorConsoleUtil.GetCountsByType();
            bool hasErrors = countsByType.errorCount - StoredErrorCount > 0;

            CompileTimeKeyframe keyframe = new CompileTimeKeyframe(elapsedTime, hasErrors);
            _Data.AddCompileTimeKeyframe(keyframe);
            KeyframeAdded.Invoke(keyframe);

            PlayClip(COMPILATION_COMPLETED_CLIP);
        }

        private static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            const string audioPlayerPreviewName = "PlayPreviewClip";
            MethodInfo method = audioUtilClass.GetMethod(audioPlayerPreviewName, BindingFlags.Static | BindingFlags.Public, null,
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