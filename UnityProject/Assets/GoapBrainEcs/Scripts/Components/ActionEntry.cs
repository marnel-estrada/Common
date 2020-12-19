using Unity.Entities;

namespace GoapBrainEcs {
    [InternalBufferCapacity(5)]
    public struct ActionEntry : IBufferElementData {
        public readonly ushort actionId;

        public ActionEntry(ushort actionId) {
            this.actionId = actionId;
        }
    }
}