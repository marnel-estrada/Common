using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapAgent : IComponentData {
        // Note here that the first goal is the main goal. Then if it can't resolve
        // actions, it will try to resolve the next goals.
        public ConditionList5 goals;
        
        // We use a separate entity here because we don't want the agent entity to get
        // bigger and thus will have less entities per archetype.
        // Note that the planner entity contains a BoolHashMap which is a big object.
        public readonly Entity plannerEntity;
        
        public readonly int domainId; // Maps to a GoapDomain

        public readonly BlobAssetReference<GoapDomainDatabase> domainDbReference;

        public GoapAgent(in BlobAssetReference<GoapDomainDatabase> domainDbReference, in int domainId, in Entity plannerEntity) {
            this.domainDbReference = domainDbReference;
            this.domainId = domainId;
            this.plannerEntity = plannerEntity;
            this.goals = new ConditionList5();
        }

        public void ClearGoals() {
            this.goals.Clear();
        }

        public void SetMainGoal(Condition condition) {
            // We can't just set at zero right away because the list does a bounds check
            // It will cause out of bounds exception if we set element at zero when the list
            // has no items yet
            if (this.goals.Count == 0) {
                AddGoal(condition);
            } else {
                this.goals[0] = condition;
            }
        }

        public void AddGoal(Condition goalCondition) {
            this.goals.Add(goalCondition);
        }

        public GoapDomain Domain {
            get {
                return this.domainDbReference.Value.domains[this.domainId];
            }
        }
    }
}