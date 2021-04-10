using Common;

using UnityEngine;

namespace GoapBrain {
    class TestSingleAction : GoapBrainTest {
        public override void Awake() {
            base.Awake();

            // Prepare domain
            GoapAction action = new GoapAction("TheOnlyAction");
            action.AddAtomAction(new DebugLogAction("Acting the only action"));
            action.AddEffect("OnlyCondition", true);
            this.Domain.AddAction(action);

            this.Domain.Configure();

            this.Agent.AddGoal("OnlyCondition", true);
            this.Agent.Replan();

            GoapActionPlan plan = this.Agent.Plan;
            Assertion.IsTrue(plan.Successful);
            Assertion.IsTrue(plan.GetActionAt(0).Name.Equals("TheOnlyAction"));

            Debug.Log("TestSingleAction:");
            PrintActions(plan);
        }
    }
}
