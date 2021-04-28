using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// The conditions map was removed from here. It's now maintained by DynamicBufferHashMap
    /// add to the same entity as the GoapPlanner
    /// </summary>
    public struct GoapPlanner : IComponentData {
        public ValueTypeOption<Condition> currentGoal;

        public readonly Entity agentEntity;

        public PlanningState state;
        
        // Index of the current goal being planned
        public int goalIndex;

        public GoapPlanner(Entity agentEntity) : this() {
            this.agentEntity = agentEntity;
        }

        /// <summary>
        /// Marks to start the planning
        /// </summary>
        /// <param name="goal"></param>
        public void StartPlanning(Condition goal) {
            this.currentGoal = ValueTypeOption<Condition>.Some(goal);
            this.state = PlanningState.RESOLVING_CONDITIONS;
        }
    }
}