using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapAgent : IComponentData {
        public readonly FixedString64 domainId; // Maps to a GoapDomain

        // Note here that the first goal is the main goal. Then if it can't resolve
        // actions, it will try to resolve the next goals.
        public ConditionList5 goals;
        
        // We use a separate entity here because we don't want the agent entity to get
        // bigger and thus will have less entities per archetype.
        // Note that the planner entity contains a BoolHashMap which is a big object.
        public readonly Entity plannerEntity;

        public GoapAgent(FixedString64 domainId, Entity plannerEntity) {
            this.domainId = domainId;
            this.plannerEntity = plannerEntity;
            this.goals = new ConditionList5();
        }

        public void ClearGoals() {
            this.goals.Clear();
        }

        public void SetMainGoal(Condition condition) {
            this.goals[0] = condition;
        }

        public void AddGoal(Condition goalCondition) {
            this.goals.Add(goalCondition);
        }
    }
}