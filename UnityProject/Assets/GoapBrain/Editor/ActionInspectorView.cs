using Common;

using UnityEditor;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    ///     Handles rendering of the action inspector part
    /// </summary>
    internal class ActionInspectorView {
        private readonly AtomActionsView atomActionsView;
        private readonly ActionConditionsView effectsView;

        private readonly ActionConditionsView preconditionsView;

        private Vector2 mainScrollPos;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ActionInspectorView(EditorWindow parent) {
            this.preconditionsView = new ActionConditionsView(parent, "Precondition", ColorUtils.RED, false);
            this.effectsView = new ActionConditionsView(parent, "Effect", ColorUtils.GREEN, false);
            this.atomActionsView = new AtomActionsView(parent);
        }

        /// <summary>
        ///     Renders the view
        /// </summary>
        public void Render(GoapDomainData domain, GoapActionData action) {
            this.mainScrollPos = GUILayout.BeginScrollView(this.mainScrollPos);

            // Remove action
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Remove Action", GUILayout.Width(100))) {
                RemoveAction(domain, action);
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            // Enabled or Disabled
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enabled: ", GUILayout.Width(100));
            action.Enabled = GUILayout.Toggle(action.Enabled, "", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Cancellable
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cancellable: ", GUILayout.Width(100));
            action.Cancellable = GUILayout.Toggle(action.Cancellable, "", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name: ", GUILayout.Width(100));
            GUILayout.Label(action.Name, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Cost
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cost: ", GUILayout.Width(100));
            action.Cost = EditorGUILayout.FloatField(action.Cost, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            // Comment
            RenderComment(action);

            GUILayout.Space(5);

            // Preconditions
            GUI.backgroundColor = ColorUtils.RED;
            GUILayout.Box("Preconditions", GUILayout.Width(100));
            GUI.backgroundColor = ColorUtils.WHITE;

            this.preconditionsView.RenderConditions(domain, action.Preconditions);

            GUILayout.Space(10);

            // Effects
            GUI.backgroundColor = ColorUtils.GREEN;
            GUILayout.Box("Effect", GUILayout.Width(70));
            GUI.backgroundColor = ColorUtils.WHITE;

            this.effectsView.RenderEffect(domain, action);

            GUILayout.Space(10);

            // Actions
            GUILayout.Box("Actions", GUILayout.Width(70));
            this.atomActionsView.Render(domain, action);

            GUILayout.EndScrollView();
        }

        private static void RenderComment(GoapActionData action) {
            action.ShowComment = EditorGUILayout.Foldout(action.ShowComment, "Comment");
            if (action.ShowComment) {
                if (action.EditComment) {
                    EditorStyles.textField.wordWrap = true;
                    action.Comment =
                        EditorGUILayout.TextArea(action.Comment, GUILayout.Width(600), GUILayout.Height(200));
                    if (GUILayout.Button("Done", GUILayout.Width(70))) {
                        action.EditComment = false;
                    }
                } else {
                    EditorGUILayout.HelpBox(action.Comment, MessageType.Info);
                    if (GUILayout.Button("Edit", GUILayout.Width(70))) {
                        action.EditComment = true;
                    }
                }
            }
        }

        private void RemoveAction(GoapDomainData domain, GoapActionData action) {
            if (EditorUtility.DisplayDialogComplex("Remove Action",
                    $"Are you sure you want to remove action \"{action.Name}\"?", "Yes", "No",
                    "Cancel") !=
                0) {
                // Cancelled or No
                return;
            }

            domain.RemoveAction(action);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }
    }
}