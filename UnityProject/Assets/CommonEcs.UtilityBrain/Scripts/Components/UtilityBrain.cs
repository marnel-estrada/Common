using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public struct UtilityBrain : IComponentData {
        public ValueTypeOption<Entity> currentBestOption;

        public readonly Entity agentEntity;
        
        // Utilities are not computed every frame
        // They are only computed when this is true
        // This will be replaced by enable/disable component when it it available.
        public bool shouldExecute;

        public UtilityBrain(Entity agentEntity) : this() {
            this.agentEntity = agentEntity;
        }
    }
}