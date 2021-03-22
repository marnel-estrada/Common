using Unity.Entities;

namespace CommonEcs.Goap {
    [InternalBufferCapacity(20)]
    public readonly struct ResolvedAction : IBufferElementData {
        public readonly int actionId;

        public ResolvedAction(int actionId) {
            this.actionId = actionId;
        }
    }
}