using Common;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace GoalSelector.Editor {
    public class GoalDataRenderer : DataPoolItemRenderer<GoalData> {
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(GoalData));
        private readonly ConsiderationsView considerationsView;

        public GoalDataRenderer(EditorWindow parent) {
            this.considerationsView = new ConsiderationsView(parent, GoalSelectorEditorWindow.REPAINT);
        }

        private Vector2 scrollPos;

        public void Render(DataPool<GoalData> pool, GoalData item) {
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

            // Remove Goal
            GUI.backgroundColor = ColorUtils.RED;
            if (GUILayout.Button("Remove Goal", GUILayout.Width(100))) {
                RemoveGoal(pool, item);
            }

            GUI.backgroundColor = ColorUtils.WHITE;

            GUILayout.Space(5);

            item.Hashcode = new FixedString64Bytes(item.Id).GetHashCode();

            this.renderer.Render(item);

            EditorGUILayout.Space(5);

            GUILayout.Label("Considerations", EditorStyles.boldLabel);
            RenderConsiderations(pool, item);

            EditorGUILayout.EndScrollView();
        }

        private void RemoveGoal(DataPool<GoalData> pool, GoalData goal) {
            if (EditorUtility.DisplayDialogComplex("Remove Goal",
                $"Are you sure you want to remove goal \"{goal.ConditionName}\"?", "Yes", "No",
                "Cancel") != 0) {
                // Cancelled or No
                return;
            }

            if (goal.Id == null) {
                throw new CantBeNullException(nameof(goal.Id));
            }

            pool.Remove(goal.Id);

            EditorUtility.SetDirty(pool);
            GoalSelectorEditorWindow.REPAINT.Dispatch();
        }

        private void RenderConsiderations(DataPool<GoalData> pool, GoalData item) {
            this.considerationsView.Render(pool, item.Considerations, pool.Skin);
        }
    }
}