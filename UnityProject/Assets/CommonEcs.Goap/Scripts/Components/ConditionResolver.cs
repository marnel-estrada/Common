using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// A component that is added to an entity to denote that it is a condition resolver
    /// A custom component may be added to such entity so that the system that will resolve
    /// the condition can filter which entities.
    /// </summary>
    public struct ConditionResolver : IComponentData {
        public readonly ConditionHashId conditionId;
        public readonly Entity agentEntity;
        
        // We denormalize here for faster access
        public readonly Entity plannerEntity;

        // This is the index to where it will write its result to its parent planner's dynamic buffer of 
        // bool results.
        // Note that the conditions map is maintained using DynamicBufferHashMap added to the GoapPlanner entity
        public readonly int resultIndex;

        public bool result;

        public ConditionResolver(ConditionHashId conditionId, Entity agentEntity, Entity plannerEntity, int resultIndex) : this() {
            this.conditionId = conditionId;
            this.agentEntity = agentEntity;
            this.plannerEntity = plannerEntity;
            this.resultIndex = resultIndex;
        }
        
        public ConditionResolver(FixedString64Bytes stringId, Entity agentEntity, Entity plannerEntity, int resultIndex) 
            : this(new ConditionHashId(stringId), agentEntity, plannerEntity, resultIndex) {
        }

        // An IEnableableComponent that identifies if a condition resolver was already resolved
        public readonly struct Resolved : IComponentData, IEnableableComponent {
        }
    }
}