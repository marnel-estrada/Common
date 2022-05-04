using System;

using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace Common {
    public class DataPoolEditorWindow<T> : EditorWindow where T : class, IDataPoolItem, IDuplicable<T>, new() {
        private DataPool<T>? target;

        public static readonly Signal.Signal REPAINT = new Signal.Signal("Repaint");

        private readonly DataPoolSidebarView<T> sidebar = new DataPoolSidebarView<T>();
        private DataPoolInspectorView<T>? inspector;

        private Action<DataPool<T>>? runAction;
        private string? runActionButtonLabel; // This is the text to show on the run button
        private int runActionButtonWidth;

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
            if (this.target == null) {
                throw new CantBeNullException(nameof(this.target));
            }
        
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
                DoBeforeSave(this.target);
                EditorUtility.SetDirty(this.target);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Save", "Save Successful", "OK");
            }

            if (this.runAction != null) {
                if (GUILayout.Button(this.runActionButtonLabel, GUILayout.Width(this.runActionButtonWidth))) {
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
            if (this.sidebar.IsValidSelection(this.target)) {
                if (this.inspector == null) {
                    throw new CantBeNullException(nameof(this.inspector));
                }
                
                this.inspector.Render(this.target, this.sidebar.GetSelectedItem(this.target));
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        public void SetRunAction(Action<DataPool<T>> action, string buttonLabel, int buttonWidth) {
            this.runAction = action;
            this.runActionButtonLabel = buttonLabel;
            this.runActionButtonWidth = buttonWidth;
        }

        /// <summary>
        /// Adds a filtering strategy
        /// </summary>
        /// <param name="strategy"></param>
        public void AddFilterStrategy(DataPoolFilterStrategy<T> strategy) {
            this.sidebar.AddFilterStrategy(strategy);
        }
        
        /// <summary>
        /// Routines that subclass may perform like verifying data before saving
        /// </summary>
        protected virtual void DoBeforeSave(DataPool<T> target) {
        }
    }
}