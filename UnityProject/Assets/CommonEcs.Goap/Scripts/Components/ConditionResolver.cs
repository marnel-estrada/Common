using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// A component that is added to an entity to denote that it is a condition resolver
    /// A custom component may be added to such entity so that the system that will resolve
    /// the condition can filter which entities.
    /// </summary>
    public readonly struct ConditionResolver : IComponentData {
        public readonly FixedString64 conditionId;
        public readonly Entity agentEntity;
    }
}