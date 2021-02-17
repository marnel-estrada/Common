using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapAgent : IComponentData {
        public readonly FixedString64 domainId; // Maps to a GoapDomain

        public ConditionList5 goals;
        public ConditionList5 fallbackGoals;

        public GoapAgent(FixedString64 domainId) {
            this.domainId = domainId;
            this.goals = new ConditionList5();
            this.fallbackGoals = new ConditionList5();
        }

        public void ClearGoals() {
            this.goals.Clear();
        }

        public void AddGoal(Condition goalCondition) {
            this.goals.Add(goalCondition);
        }

        public void ClearFallbackGoals() {
            this.fallbackGoals.Clear();
        }

        public void AddFallbackGoal(Condition fallbackCondition) {
            this.fallbackGoals.Add(fallbackCondition);
        }
    }
}