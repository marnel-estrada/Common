using UnityEngine;

namespace Common {
    /// <summary>
    /// Destroys the target objects if the specified game variable is false
    /// </summary>
    public class DestroyByGameVariable : MonoBehaviour {
        [SerializeField]
        private string gameVariableId;
        
        [SerializeField]
        private GameObject[] targets;

        private void Start() {
            Assertion.AssertNotEmpty(this.gameVariableId);
            Assertion.Assert(this.targets.Length > 0);

            bool value = GameVariablesQuery.GET_BOOL_GAME_VARIABLE.Execute(this.gameVariableId);
            if (!value) {
                // Destroy targets
                for (int i = 0; i < this.targets.Length; ++i) {
                    DestroyImmediate(this.targets[i]);
                }
            }
        }
    }
}