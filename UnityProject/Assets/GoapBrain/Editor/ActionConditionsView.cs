using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Common;

namespace GoapBrain {
    class ActionConditionsView {

        private readonly EditorWindow parent;
        private readonly string itemName;
        private readonly Color backgroundColor;
        private readonly bool includeExtensions;

        private string newConditionName = "";
        private bool newConditionValue = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionConditionsView(EditorWindow parent, string itemName, Color backgroundColor, bool includeExtensions) {
            this.parent = parent;
            this.itemName = itemName;
            this.backgroundColor = backgroundColor;
            this.includeExtensions = includeExtensions;
        }

        /// <summary>
        /// Renders a list of conditions
        /// </summary>
        /// <param name="conditionList"></param>
        public void Render(GoapDomainData domain, List<Condition> conditionList) {
            // New condition
            RenderAddNewCondition(domain, conditionList);

            GUILayout.Space(5);

            // Render each
            if (conditionList.Count <= 0) {
                // Empty
                GUILayout.Label(string.Format("(Empty)"));
            } else {
                for (int i = 0; i < conditionList.Count; ++i) {
                    RenderCondition(domain, conditionList, conditionList[i], i);
                }
            }
        }
        
        private readonly GUIContent chooseGuiContent = new GUIContent("Choose...");

        private void RenderAddNewCondition(GoapDomainData domain, List<Condition> conditionList) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("New:", GUILayout.Width(40), GUILayout.Height(20));

            GUI.backgroundColor = this.backgroundColor;
            GUILayout.Box(string.IsNullOrEmpty(this.newConditionName) ? "(none selected)" : this.newConditionName,
                GUILayout.Width(200));
            GUI.backgroundColor = Color.white;

            Rect buttonRect = GUILayoutUtility.GetRect(this.chooseGuiContent, GUI.skin.button, GUILayout.Width(70), GUILayout.Height(20));
            if (GUI.Button(buttonRect, "Choose...")) {
                GoapEditorUtils.OpenConditionSelector(domain, this.parent, OnConditionSelected, this.includeExtensions);
            }

            GUILayout.Space(5);
            
            this.newConditionValue = GUILayout.Toggle(this.newConditionValue, this.newConditionValue ? "true" : "false", EditorStyles.miniButton, GUILayout.Width(50), GUILayout.Height(20));

            GUILayout.Space(5);

            GUI.backgroundColor = ColorUtils.GREEN;
            if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(20))) {
                AddNewCondition(domain, conditionList);
            }
            GUI.backgroundColor = ColorUtils.WHITE;
            GUILayout.EndHorizontal();
        }

        private void RenderCondition(GoapDomainData domain, List<Condition> conditionList, Condition condition, int index) {
            GUILayout.BeginHorizontal();

            // Remove button
            GUI.backgroundColor = ColorUtils.RED;
            if(GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20))) {
                RemoveCondition(domain, conditionList, condition);
            }
            GUI.backgroundColor = ColorUtils.WHITE;

            if(GUILayout.Button("Up", GUILayout.Width(35), GUILayout.Height(20))) {
                MoveUp(domain, conditionList, index);
            }

            if (GUILayout.Button("Down", GUILayout.Width(45), GUILayout.Height(20))) {
                MoveDown(domain, conditionList, index);
            }

            // Name
            GUI.backgroundColor = this.backgroundColor;
            GUILayout.Box(condition.Name, GUILayout.Width(400), GUILayout.Height(20));
            GUI.backgroundColor = ColorUtils.WHITE;

            // Value
            condition.Value = GUILayout.Toggle(condition.Value, condition.Value ? "true" : "false", EditorStyles.miniButton, GUILayout.Width(50), GUILayout.Height(20));

            GUILayout.EndHorizontal();
        }

        private void OnConditionSelected(string conditionName) {
            this.newConditionName = conditionName;
        }

        private void AddNewCondition(GoapDomainData domain, List<Condition> conditionList) {
            if(string.IsNullOrEmpty(this.newConditionName)) {
                // No condition currently chosen
                return;
            }

            // Check if the same condition already exists

            Condition condition = new Condition(this.newConditionName, this.newConditionValue);
            conditionList.Add(condition);

            this.newConditionName = "";

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void RemoveCondition(GoapDomainData domain, List<Condition> conditionList, Condition condition) {
            if(EditorUtility.DisplayDialogComplex(string.Format("Remove {0}", this.itemName), string.Format("Are you sure you want to remove {0} \"{1}\"", this.itemName, condition.Name), 
                "Yes", "No", "Cancel") != 0) {
                // Cancelled or selected No
                return;
            }

            conditionList.Remove(condition);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void MoveUp(GoapDomainData domain, List<Condition> conditionList, int index) {
            if(index <= 0) {
                // Can no longer move up
                return;
            }

            Swap(conditionList, index - 1, index);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private void MoveDown(GoapDomainData domain, List<Condition> conditionList, int index) {
            if (index + 1 >= conditionList.Count) {
                // Can no longer move down
                return;
            }

            Swap(conditionList, index, index + 1);

            EditorUtility.SetDirty(domain);
            GoapEditorSignals.REPAINT.Dispatch();
        }

        private static void Swap(List<Condition> conditionList, int a, int b) {
            Condition temp = conditionList[a];
            conditionList[a] = conditionList[b];
            conditionList[b] = temp;
        }

    }
}
