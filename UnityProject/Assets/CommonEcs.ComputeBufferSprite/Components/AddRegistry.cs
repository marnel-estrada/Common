using Unity.Entities;

namespace CommonEcs {
    public readonly struct AddRegistry : ISystemStateComponentData {
        public readonly Entity drawInstanceEntity;
        public readonly int masterListIndex;

        public AddRegistry(Entity drawInstanceEntity, int masterListIndex) {
            this.drawInstanceEntity = drawInstanceEntity;
            this.masterListIndex = masterListIndex;
        }
    }
}