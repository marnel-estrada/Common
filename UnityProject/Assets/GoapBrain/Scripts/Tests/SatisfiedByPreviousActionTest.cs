using Common;

using UnityEngine;

namespace GoapBrain {
    class SatisfiedByPreviousActionTest : GoapBrainTest {

        // Actions
        private const string FIND_METAL_ORE = "FindMetalOre";
        private const string MAKE_AXE = "MakeAxe";
        private const string GET_AXE = "GetAxe";
        private const string CHOP_LOG = "ChopLog";
        private const string COLLECT_BRANCHES = "CollectBranches";

        // Conditions
        private const string HAS_METAL_ORE = "HasMetalOre";
        private const string AXE_AVAILABLE = "AxeAvailable";
        private const string HAS_AXE = "HasAxe";
        private const string HAS_FIREWOOD = "HasFirewood";

        public override void Awake() {
            base.Awake();

            // Prepare domain
            GoapAction findMetalOre = new GoapAction(FIND_METAL_ORE);
            findMetalOre.AddAtomAction(new DebugLogAction(FIND_METAL_ORE));
            findMetalOre.AddEffect(HAS_METAL_ORE, true);
            findMetalOre.AddEffect(AXE_AVAILABLE, true); // Ooops we found an axe, this should skip MakeAxe
            findMetalOre.Cost = 2;
            Domain.AddAction(findMetalOre);

            GoapAction makeAxe = new GoapAction(MAKE_AXE);
            makeAxe.AddPrecondition(HAS_METAL_ORE, true);
            makeAxe.AddAtomAction(new DebugLogAction(MAKE_AXE));
            makeAxe.AddEffect(AXE_AVAILABLE, true);
            makeAxe.Cost = 1;
            Domain.AddAction(makeAxe);

            GoapAction getAxe = new GoapAction(GET_AXE);
            getAxe.AddPrecondition(AXE_AVAILABLE, true);
            getAxe.AddPrecondition(HAS_AXE, false);
            getAxe.AddAtomAction(new DebugLogAction(GET_AXE));
            getAxe.AddEffect(HAS_AXE, true);
            getAxe.Cost = 1;
            Domain.AddAction(getAxe);

            GoapAction chopLog = new GoapAction(CHOP_LOG);
            chopLog.AddPrecondition(HAS_AXE, true);
            chopLog.AddAtomAction(new DebugLogAction(CHOP_LOG));
            chopLog.AddEffect(HAS_FIREWOOD, true);
            chopLog.Cost = 1;
            Domain.AddAction(chopLog);

            GoapAction collectBranches = new GoapAction(COLLECT_BRANCHES);
            collectBranches.AddAtomAction(new DebugLogAction(COLLECT_BRANCHES));
            collectBranches.AddEffect(HAS_FIREWOOD, true);
            collectBranches.Cost = 8;
            Domain.AddAction(collectBranches);

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
