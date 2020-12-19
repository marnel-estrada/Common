using Unity.Entities;

namespace GoapBrainEcs {
    public struct GoapAgent : IComponentData {
        public readonly ushort domainId; // Maps to a GoapDomain

        // The entity with the GoapPlanRequest
        // When search completes, we use this value to check if it's the same
        // If it is, the agent will use the actions in this request
        // If not, a system will identify this and remove all planning entities related to such request 
        public Entity currentRequest;
        
        public ConditionList5 goals;
        public ConditionList5 fallbackGoals;

        public GoapAgent(ushort domainId) {
            this.domainId = domainId;
            this.currentRequest = Entity.Null;
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