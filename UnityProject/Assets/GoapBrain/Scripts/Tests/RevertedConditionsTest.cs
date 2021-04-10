using Common;

using UnityEngine;

namespace GoapBrain {
    class RevertedConditionsTest : GoapBrainTest {

        // Actions
        private const string A = "A";
        private const string A1 = "A1";
        private const string A1a = "A1a";
        private const string A1b = "A1b";
        private const string A1c = "A1c";

        private const string A2 = "A2";
        private const string A2a = "A2a";
        private const string A2b = "A2b";
        private const string A2c = "A2c";

        private const string B = "B";

        // Conditions
        private const string cA = "cA";

        private const string cA1 = "cA1";
        private const string cA1a = "cA1a";
        private const string cA1b = "cA1b";
        private const string cA1c = "cA1c";
        private const string CHECK_CONDITION = "CheckCondition";

        private const string cA2 = "cA2";
        private const string cA2a = "cA2a";
        private const string cA2b = "cA2b"; // No action for this
        private const string cA2c = "cA2c";

        public override void Awake() {
            base.Awake();

            // Prepare domain
            PrepareAActions();

            // Prepare B action
            {
                GoapAction action = CreateAction(B, cA, true);
                action.AddPrecondition(CHECK_CONDITION, true);
                action.Cost = 2;
            }

            Domain.AddPreconditionResolver(CHECK_CONDITION, new ConstantResolver(true)); // CheckCondition defaults to true
            Domain.Configure();

            Agent.ClearGoals();
            Agent.AddGoal(cA, true);
            Agent.Replan();

            Assertion.IsTrue(Agent.Plan.Successful);

            Debug.Log("RevertedConditionsTest");
            PrintActions(Agent.Plan);
        }

        private void PrepareAActions() {
            {
                GoapAction action = CreateAction(A, cA, true);
                action.AddPrecondition(cA1, true);
                action.AddPrecondition(cA2, true);
                action.Cost = 1;
            }

            {
                GoapAction action = CreateAction(A1, cA1, true);
                action.AddPrecondition(cA1a, true);
                action.AddPrecondition(cA1b, true);
                action.AddPrecondition(cA1c, true);
            }

            {
                // Note here that this already adds the action to the domain
                CreateAction(A1a, cA1a, true);
                CreateAction(A1b, cA1b, true);

                // The special one. Has two effects
                GoapAction action = CreateAction(A1c, cA1c, true);
                action.AddEffect(CHECK_CONDITION, false);
            }

            {
                GoapAction action = CreateAction(A2, cA2, true);
                action.AddPrecondition(cA2a, true);
                action.AddPrecondition(cA2b, true);
                action.AddPrecondition(cA2c, true);
            }

            {
                CreateAction(A2a, cA2a, true);
                CreateAction(A2b, cA2b, true); // Comment this out to make this action branch fail
                CreateAction(A2c, cA2c, true);
            }
        }

    }
}
