using Common;

using UnityEngine;

namespace GoapBrain {
    class ChainedActionsTest : GoapBrainTest {

        [SerializeField]
        private bool axeAvailable = true;

        // Actions
        private const string GET_AXE = "GetAxe";
        private const string CHOP_LOG = "ChopLog";
        private const string COLLECT_BRANCHES = "CollectBranches";

        // Conditions
        private const string AXE_AVAILABLE = "AxeAvailable";
        private const string HAS_AXE = "HasAxe";
        private const string HAS_FIREWOOD = "HasFirewood";

        public override void Awake() {
            base.Awake();

            // Prepare domain
            GoapAction getAxe = new GoapAction(GET_AXE);
            getAxe.AddPrecondition(AXE_AVAILABLE, true);
            getAxe.AddPrecondition(HAS_AXE, false);
            getAxe.AddAtomAction(new DebugLogAction(GET_AXE));
            getAxe.AddEffect(HAS_AXE, true);
            getAxe.Cost = 2;
            Domain.AddAction(getAxe);

            GoapAction chopLog = new GoapAction(CHOP_LOG);
            chopLog.AddPrecondition(HAS_AXE, true);
            chopLog.AddAtomAction(new DebugLogAction(CHOP_LOG));
            chopLog.AddEffect(HAS_FIREWOOD, true);
            chopLog.Cost = 4;
            Domain.AddAction(chopLog);

            GoapAction collectBranches = new GoapAction(COLLECT_BRANCHES);
            collectBranches.AddAtomAction(new DebugLogAction(COLLECT_BRANCHES));
            collectBranches.AddEffect(HAS_FIREWOOD, true);
            collectBranches.Cost = 8;
            Domain.AddAction(collectBranches);

            Domain.AddPreconditionResolver(AXE_AVAILABLE, new ConstantResolver(this.axeAvailable));

            Domain.Configure();

            Agent.ClearGoals();
            Agent.AddGoal(HAS_FIREWOOD, true);
            Agent.Replan();

            GoapActionPlan plan = Agent.Plan;
            Assertion.IsTrue(plan.Successful);

            Debug.Log("ChainedActionsTest:");
            PrintActions(plan);
        }

    }
}
