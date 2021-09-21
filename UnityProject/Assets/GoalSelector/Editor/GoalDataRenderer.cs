using Common;
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

            this.renderer.Render(item);

            EditorGUILayout.Space(5);

            GUILayout.Label("Considerations", EditorStyles.boldLabel);
            RenderConsiderations(pool, item);

            EditorGUILayout.EndScrollView();
        }

        private void RenderConsiderations(DataPool<GoalData> pool, GoalData item) {
            this.considerationsView.Render(pool, item.Considerations, pool.Skin);
        }
    }
}