using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// An index into a UV array. We implemented this as an IBufferElementData as we may want multiple
    /// UVs like in case of heads in Academia: School Simulator.
    /// </summary>
    [InternalBufferCapacity(1)]
    public struct UvIndex : IBufferElementData {
        public readonly int value;

        public UvIndex(int value) {
            this.value = value;
        }
    }
}