using System.Collections.Generic;

using Common;

using UnityEditor;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    ///     Handles rendering of conditions part
    /// </summary>
    internal class ConditionsView {
        private string newConditionName = "";

        private Vector2 scrollPos;

        /// <summary>
        ///     Renders the UI
        /// </summary>
        /// <param name="domain"></param>
        public void Render(GoapDomainData domain) {
            EditorGUILayout.BeginVertical();

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

            GUILayout.Label("Conditions", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Add section
            GUILayout.Label("New condition:");
            GUILayout.BeginHorizontal();
            this.newConditionName = EditorGUILayout.TextField(this.newConditionName, GUILayout.Width(200));
            if (GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(15))) {
                AddConditionName(domain, this.newConditionName);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Condition names
            RenderConditionNames(domain);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void RenderConditionNames(GoapDomainData domain) {
            if (domain.ConditionNamesCount <= 0) {
                // No names at the moment
                GUILayout.Label("(no condition names yet)");

                return;
            }

            for (int i = 0; i < domain.ConditionNamesCount; ++i) {
                ConditionName name = domain.GetConditionNameAt(i);

                GUILayout.BeginHorizontal();

                if (name.RenameMode) {
                    name.NewName = EditorGUILayout.TextField(name.NewName, GUILayout.Width(200));
                    if (GUILayout.Button("Done", GUILayout.Width(40), GUILayout.Height(15))) {
                        Rename(domain, name);
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(50), GUILayout.Height(15))) {
                        name.RenameMode = false;
                    }
                } else {
                    GUI.backgroundColor = ColorUtils.RED;
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(15))) {
                        TryRemoveConditionName(domain, name);
                    }

                    GUI.backgroundColor = ColorUtils.WHITE;

                    if (GUILayout.Button("Rename", GUILayout.Width(60), GUILayout.Height(15))) {
                        name.NewName = name.Name;
                        name.RenameMode = true;
                    }

                    GUILayout.Label(name.Name, GUILayout.Width(400));
                }

                GUILayout.EndHorizontal();
            }
        }

        private void AddConditionName(GoapDomainData data, string conditionName) {
            if (string.IsNullOrEmpty(conditionName)) {
                // Can't add. Empty name
                return;
            }

            // Check if it already exists
            ConditionName? name = data.GetConditionName(conditionName);
            if (name != null) {
                // An action with the same name already exists
                EditorUtility.DisplayDialog("Can't add", "A condition with the same name already exists.", "OK");

                return;
            }

            data.AddConditionName(conditionName);

            // Return to blank so user could type a new action name again
            this.newConditionName = "";

            EditorUtility.SetDirty(data);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void Rename(GoapDomainData domain, ConditionName conditionName) {
            if (conditionName.Name.Equals(conditionName.NewName)) {
                // User did not rename at all
                conditionName.RenameMode = false;

                return;
            }

            // Check if new name is already an existing condition
            for (int i = 0; i < domain.ConditionNamesCount; ++i) {
                ConditionName name = domain.GetConditionNameAt(i);
                if (name.Name.Equals(conditionName.NewName)) {
                    // A ConditionName with the same name as the new one already exists
                    EditorUtility.DisplayDialog("Rename Condition",
                        "Can't rename. A condition with the same name as the new name already exists.", "OK");

                    return;
                }
            }

            // Look for actions and rename associated preconditions and effects
            for (int i = 0; i < domain.ActionCount; ++i) {
                RenameConditions(domain.GetActionAt(i), conditionName.Name, conditionName.NewName);
            }

            // Rename condition resolvers
            for (int i = 0; i < domain.ConditionResolvers.Count; ++i) {
                ConditionResolverData resolverData = domain.ConditionResolvers[i];
                if (resolverData.ConditionName.Equals(conditionName.Name)) {
                    resolverData.ConditionName = conditionName.NewName;
                }
            }

            // Renamed preconditions in extensions
            for (int i = 0; i < domain.Extensions.Count; ++i) {
                GoapExtensionData extension = domain.Extensions[i];
                RenameConditions(extension, conditionName.Name, conditionName.NewName);
            }

            // Finally, rename the condition name itself
            conditionName.Name = conditionName.NewName;
            conditionName.RenameMode = false;
        }

        private void RenameConditions(GoapExtensionData extension, string oldName, string newName) {
            for (int i = 0; i < extension.Preconditions.Count; ++i) {
                Condition condition = extension.Preconditions[i];
                if (condition.Name.EqualsFast(oldName)) {
                    condition.Name = newName;
                }
            }
        }

        private void RenameConditions(GoapActionData action, string oldName, string newName) {
            // Rename preconditions
            for (int i = 0; i < action.Preconditions.Count; ++i) {
                Condition condition = action.Preconditions[i];
                if (condition.Name.EqualsFast(oldName)) {
                    condition.Name = newName;
                }
            }

            // Rename effect
            if (action.Effect == null) {
                return;
            }

            if (action.Effect.Name.EqualsFast(oldName)) {
                action.Effect.Name = newName;
            }
        }

        private void TryRemoveConditionName(GoapDomainData domain, ConditionName conditionName) {
            if (EditorUtility.DisplayDialogComplex("Remove Condition",
                $"Are you sure you want to remove condition \"{conditionName.Name}\"?", "Yes", "No",
                "Cancel") != 0) {
                // Deletion cancelled or selected No
                return;
            }

            // Remove preconditions and effects
            for (int i = 0; i < domain.ActionCount; ++i) {
                RemoveAssociatedConditions(domain.GetActionAt(i), conditionName);
            }

            // Remove resolver
            ConditionResolverData? resolver = domain.GetConditionResolver(conditionName.Name);
            if (resolver != null) {
                domain.RemoveConditionResolver(resolver);
            }

            domain.RemoveConditionName(conditionName);
            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private static void RemoveAssociatedConditions(GoapActionData action, ConditionName conditionName) {
            // Remove from preconditions, there can only be one
            Condition? found = FindCondition(action.Preconditions, conditionName);
            if (found != null) {
                action.Preconditions.Remove(found);
            }

            // Remove effect if the name is the same
            if (action.Effect != null && action.Effect.Name.EqualsFast(conditionName.Name)) {
                action.Effect = null;
            }
        }

        private static Condition? FindCondition(IReadOnlyList<Condition> conditionList, ConditionName conditionName) {
            for (int i = 0; i < conditionList.Count; ++i) {
                if (conditionList[i].Name.EqualsFast(conditionName.Name)) {
                    return conditionList[i];
                }
            }

            // Client code should check for this
            return null;
        }
    }
}