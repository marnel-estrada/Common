using UnityEngine;
using Common;

namespace GoapBrain {
    public class TestAgent : MonoBehaviour {

        [SerializeField]
        private Condition goal = new Condition("HasFuel", true);

        [SerializeField]
        private GoapAgent agent;

        private void Awake() {
            Assertion.NotNull(this.agent);
            this.agent.AddGoal(this.goal.Name, this.goal.Value);
            this.agent.Replan();
        }

    }
}
