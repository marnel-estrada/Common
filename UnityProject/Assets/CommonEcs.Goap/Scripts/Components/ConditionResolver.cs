using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// A component that is added to an entity to denote that it is a condition resolver
    /// A custom component may be added to such entity so that the system that will resolve
    /// the condition can filter which entities.
    /// </summary>
    public struct ConditionResolver : IComponentData {
        public readonly ConditionId conditionId;
        public readonly Entity agentEntity;
        
        // We denormalize here for faster access
        public readonly Entity plannerEntity;

        public bool resolved;
        public bool result;

        public ConditionResolver(ConditionId conditionId, Entity agentEntity, Entity plannerEntity) : this() {
            this.conditionId = conditionId;
            this.agentEntity = agentEntity;
            this.plannerEntity = plannerEntity;
        }
        
        public ConditionResolver(FixedString32 stringId, Entity agentEntity, Entity plannerEntity) 
            : this(new ConditionId(stringId), agentEntity, plannerEntity) {
        }
        
        public ConditionResolver(FixedString64 stringId, Entity agentEntity, Entity plannerEntity) 
            : this(new ConditionId(stringId), agentEntity, plannerEntity) {
        }
    }
}