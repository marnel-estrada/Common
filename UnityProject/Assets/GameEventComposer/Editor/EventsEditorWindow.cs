using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class EventsEditorWindow : EditorWindow {
        private EventsPool target;
        
        private void OnEnable () {
            EventsEditorSignals.REPAINT.AddListener(Repaint);
        }

        private void OnDisable () {
            EventsEditorSignals.REPAINT.RemoveListener(Repaint);
        }

        public void Init(EventsPool pool) {
            this.target = pool;
        }
        
        private void OnGUI () {
            if(this.target == null) {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Events Editor: (Missing GrantsData)", EditorStyles.largeLabel);
                GUILayout.Space(10);
                return;
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Events Editor: " + this.target.name, EditorStyles.largeLabel);
            GUILayout.Space(10);

            if(GUILayout.Button("Save", GUILayout.Width(40))) {
                EditorUtility.SetDirty(this.target);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Save", "Save Successful", "OK");
            }

            GUILayout.Space(10);

            // TODO render here

            EditorGUILayout.EndVertical();
        }

        private void Repaint(ISignalParameters parameters) {
            //this.grantsView.OnRepaint(this.target);

            Repaint();
        }
    }
}