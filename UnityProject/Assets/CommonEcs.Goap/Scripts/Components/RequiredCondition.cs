using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// Represents a condition that needs to be resolved.
    /// </summary>
    [InternalBufferCapacity(50)]
    public readonly struct RequiredCondition : IBufferElementData {
        public readonly ConditionHashId conditionId;

        public RequiredCondition(ConditionHashId conditionId) {
            this.conditionId = conditionId;
        }
    }
}