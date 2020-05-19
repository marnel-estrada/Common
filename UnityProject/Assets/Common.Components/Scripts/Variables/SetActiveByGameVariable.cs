using UnityEngine;

using Common;

namespace Game {
    public class SetActiveByGameVariable : MonoBehaviour {
        [SerializeField]
        private string gameVariableId;

        [SerializeField]
        private bool active; // What to set when game variable is true

        [SerializeField]
        private GameObject[] targets;

        private void Start() {
            Assertion.NotEmpty(this.gameVariableId);
            Assertion.IsTrue(this.targets.Length > 0);

            BoolGameVariable variable = new BoolGameVariable(this.gameVariableId);
            if(variable.Value) {
                SetTargets(this.active);
            } else {
                // Set the opposite
                SetTargets(!this.active);
            }
        }

        private void SetTargets(bool active) {
            for(int i = 0; i < targets.Length; ++i) {
                this.targets[i].SetActive(active);
            }
        }
    }
}
