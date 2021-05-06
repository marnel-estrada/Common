using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Contains common consideration data.
    /// </summary>
    public struct Consideration : IComponentData {
        // We denormalize here for faster access when processing consideration entities
        public UtilityValue value;
        
        // We denormalize here for faster access
        // We may need this to get data from the agent entity
        public readonly Entity agentEntity;

        // The pointer entity to its option parent
        public readonly Entity optionEntity;

        // This is the index of the DynamicBuffer of the owner Option where we write the 
        // utility value upon processing.
        public readonly int optionIndex;

        // Note all consideration execute all the time
        // This will be replaced by enable/disable component when it it available.
        public bool shouldExecute;

        public Consideration(in Entity agentEntity, in Entity optionEntity, int optionIndex) : this() {
            this.agentEntity = agentEntity;
            this.optionEntity = optionEntity;
            this.optionIndex = optionIndex;
        }
    }
}