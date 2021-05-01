using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// This will be the values that will be maintained as DynamicBuffer associated with the
    /// Option entity.
    /// </summary>
    [InternalBufferCapacity(64)]
    public readonly struct UtilityValue : IBufferElementData {
        public readonly int rank;
        public readonly float bonus;
        public readonly float multiplier;

        public UtilityValue(int rank, float bonus, float multiplier) {
            this.rank = rank;
            this.bonus = bonus;
            this.multiplier = multiplier;
        }
    }
}