using Common;
using Common.Signal;

using UnityEditor;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    ///     The main window for the editor of GoapDomain
    /// </summary>
    public class GoapDomainEditorWindow : EditorWindow {
        private const int CONDITIONS_TAB = 0;
        private const int VARIABLES_TAB = 1;
        private const int ACTIONS_TAB = 2;
        private const int RESOLVERS_TAB = 3;
        private const int EXTENSIONS_TAB = 4;
        
        private readonly ActionsView actionsView;
        private readonly ConditionResolversView conditionResolversView;
        private readonly ConditionsView conditionsView = new();
        private readonly ExtensionsView extensions;

        private readonly string[] tabs = {
            "Conditions", "Variables", "Actions", "Resolvers", "Extensions"
        };

        private int tabSelected;

        private GoapDomainData? target;
        private readonly VariablesView variablesView = new();

        /// <summary>
        ///     Constructor
        /// </summary>
        public GoapDomainEditorWindow() {
            this.actionsView = new ActionsView(this);
            this.conditionResolversView = new ConditionResolversView(this);
            this.extensions = new ExtensionsView(this);
        }

        /// <summary>
        ///     Initializer
        /// </summary>
        /// <param name="target"></param>
        public void Init(GoapDomainData target) {
            this.target = target;
            Assertion.NotNull(this.target);

            this.actionsView.Start(target);
        }

        private void OnEnable() {
            // A Unity error told us to do this
            this.actionsView.OnEnable();
            
            GoapEditorSignals.REPAINT.AddListener(Repaint);
        }

        private void OnDisable() {
            GoapEditorSignals.REPAINT.RemoveListener(Repaint);
        }

        private void OnGUI() {
            if (!this.target) {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("GOAP Domain Editor: (Missing GoapDomainData)", EditorStyles.largeLabel);
                GUILayout.Space(10);

                return;
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Label("GOAP Domain Editor: " + this.target.name, EditorStyles.largeLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Save", GUILayout.Width(40))) {
                EditorUtility.SetDirty(this.target);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Save", "Save Successful", "OK");
            }

            // Tab buttons
            this.tabSelected = GUILayout.Toolbar(this.tabSelected, this.tabs, GUILayout.Width(400));

            GUILayout.Space(10);

            switch (this.tabSelected) {
                case CONDITIONS_TAB:
                    this.conditionsView.Render(this.target);

                    break;

                case VARIABLES_TAB:
                    this.variablesView.Render(this.target);

                    break;

                case ACTIONS_TAB:
                    this.actionsView.Render(this.target);

                    break;

                case RESOLVERS_TAB:
                    this.conditionResolversView.Render(this.target);

                    break;

                case EXTENSIONS_TAB:
                    this.extensions.Render(this.target);

                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void Repaint(ISignalParameters parameters) {
            this.actionsView.OnRepaint(this.target);

            Repaint();
        }
    }
}