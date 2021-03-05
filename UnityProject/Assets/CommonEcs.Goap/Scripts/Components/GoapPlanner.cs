using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapPlanner : IComponentData {
        // Holds the condition values
        public BoolHashMap conditionsMap;

        public Condition goal;

        public readonly Entity agentEntity;

        public PlannerState state;

        public GoapPlanner(Entity agentEntity) : this() {
            this.agentEntity = agentEntity;
        }

        /// <summary>
        /// Marks to start the planning
        /// </summary>
        /// <param name="goal"></param>
        public void StartPlanning(Condition goal) {
            this.goal = goal;
            this.conditionsMap.Clear();
            this.state = PlannerState.RESOLVING_CONDITIONS;
        }
    }
}