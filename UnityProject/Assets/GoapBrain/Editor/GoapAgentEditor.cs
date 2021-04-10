using Common;

using UnityEngine;
using UnityEditor;

namespace GoapBrain {
    [CustomEditor(typeof(GoapAgent))]
    class GoapAgentEditor : Editor {

        private GoapAgent agent;

        void OnEnable() {
            this.agent = this.target as GoapAgent;
            Assertion.NotNull(this.agent);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if(Application.isPlaying) {
                DisplayPlan();
            }
        }

        private void DisplayPlan() {
            GUILayout.BeginVertical();
            GUILayout.Label("Current Plan:", EditorStyles.boldLabel);

            GoapActionPlan plan = this.agent.Plan;
            if (plan != null) {
                for (int i = 0; i < plan.ActionCount; ++i) {
                    GUILayout.Label("- " + plan.GetActionAt(i).Name);
                }
            }

            GUILayout.EndVertical();
        }

    }
}
