using System;

using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolEditorWindow<T> : EditorWindow where T : class, IDataPoolItem, IDuplicable<T>, new() {
        private DataPool<T> target;

        public static readonly Signal.Signal REPAINT = new Signal.Signal("Repaint");

        private readonly DataPoolSidebarView<T> sidebar = new DataPoolSidebarView<T>();
        private DataPoolInspectorView<T> inspector;

        private Action<DataPool<T>> runAction;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="target"></param>
        public virtual void Init(DataPool<T> target, DataPoolItemRenderer<T> itemRenderer) {
            this.target = target;
            this.inspector = new DataPoolInspectorView<T>(this, this.sidebar, itemRenderer);
            REPAINT.Dispatch(); // Force a repaint
        }

        private void OnEnable() {
            REPAINT.AddListener(Repaint);
        }

        private void OnDisable() {
            REPAINT.RemoveListener(Repaint);
        }

        private void Repaint(ISignalParameters parameters) {
            this.sidebar.OnRepaint(this.target);
            Repaint();
        }

        private void OnGUI() {
            if (this.target == null) {
                GUILayout.BeginVertical();
                GUILayout.Label("Data Class Editor: (Missing DataClassPool target)", EditorStyles.largeLabel);
                GUILayout.Space(10);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginVertical();

            GUILayout.Label("Data Class Editor: " + this.target.name, EditorStyles.largeLabel);
            GUILayout.Space(10);

            // Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save", GUILayout.Width(40))) {
                EditorUtility.SetDirty(this.target);
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

        public Action<DataPool<T>> RunAction {
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
        public void AddFilterStrategy(DataPoolFilterStrategy<T> strategy) {
            this.sidebar.AddFilterStrategy(strategy);
        }
    }
}