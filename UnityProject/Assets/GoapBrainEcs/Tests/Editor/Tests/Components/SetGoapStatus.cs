using Unity.Entities;

namespace GoapBrainEcs {
    public readonly struct SetGoapStatus : IComponentData {
        public readonly GoapStatus status;

        public SetGoapStatus(GoapStatus status) {
            this.status = status;
        }
    }
}