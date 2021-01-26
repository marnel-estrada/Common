using System;
using UnityEngine;
using UnityEditor;
using Common.Signal;
using UnityEditor.Experimental.SceneManagement;

namespace Common {
    /// <summary>
    /// Generic editor for the DataClassPool component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataClassPoolEditorWindow<T> : EditorWindow where T : Identifiable, new() {
        private DataClassPool<T> target;

        public static readonly Signal.Signal REPAINT = new Signal.Signal("Repaint");

        private readonly DataClassSidebarView<T> sidebar = new DataClassSidebarView<T>();
        private DataClassInspectorView<T> inspector;

        private Action<DataClassPool<T>> runAction;

        private string prefabPath;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="target"></param>
        public void Init(DataClassPool<T> target, DataClassItemRenderer<T> itemRenderer) {
            this.target = target;
            this.inspector = new DataClassInspectorView<T>(itemRenderer);
            this.prefabPath = PrefabStageUtility.GetPrefabStage(this.target.gameObject)?.prefabAssetPath;
            REPAINT.Dispatch(); // Force a repaint
        }

        void OnEnable() {
            REPAINT.AddListener(Repaint);
        }

        void OnDisable() {
            REPAINT.RemoveListener(Repaint);
        }

        private void Repaint(ISignalParameters parameters) {
            this.sidebar.OnRepaint(this.target);
            Repaint();
        }

        void OnGUI() {
            if (this.target == null) {
                GUILayout.BeginVertical();
                GUILayout.Label("Data Class Pool Editor: (Missing DataClassPool target)", EditorStyles.largeLabel);
                GUILayout.Space(10);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginVertical();

            GUILayout.Label("Data Class Pool Editor: " + this.target.name, EditorStyles.largeLabel);
            GUILayout.Space(10);

            // Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save", GUILayout.Width(40))) {
                EditorUtility.SetDirty(this.target);

                if (!string.IsNullOrEmpty(this.prefabPath)) {
                    PrefabUtility.SaveAsPrefabAsset(this.target.gameObject, this.prefabPath);
                }

                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("Save", "Save Successful", "OK");
            }

            if (this.runAction != null) {
                if (GUILayout.Button("Run", GUILayout.Width(40))) {
                    this.runAction(this.target);
                    EditorUtility.DisplayDialog("Run", "Run Executed", "OK");
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            // Sidebar
            this.sidebar.Render(this.target);

            GUILayout.Space(10);

            // Inspector
            if (this.sidebar.IsValidSelection) {
                this.inspector.Render(this.target, this.sidebar.SelectedItem);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        public Action<DataClassPool<T>> RunAction {
            get {
                return this.runAction;
            }

            set {
                this.runAction = value;
            }
        }

        /// <summary>
        /// Adds a filtering strategy
        /// </summary>
        /// <param name="strategy"></param>
        public void AddFilterStrategy(DataClassFilterStrategy<T> strategy) {
            this.sidebar.AddFilterStrategy(strategy);
        }
    }
}