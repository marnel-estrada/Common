using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapPlanner : IComponentData {
        // Holds the condition values
        public BoolHashMap conditionsMap;

        public Condition currentGoal;

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
            this.currentGoal = goal;
            this.conditionsMap.Clear();
            this.state = PlanningState.RESOLVING_CONDITIONS;
        }
    }
}