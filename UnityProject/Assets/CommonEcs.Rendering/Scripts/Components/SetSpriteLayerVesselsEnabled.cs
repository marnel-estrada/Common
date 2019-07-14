using Unity.Entities;

namespace CommonEcs {
    public readonly struct SetSpriteLayerVesselsEnabled : IComponentData {
        public readonly Entity spriteLayerEntity;
        public readonly bool enabled;

        public SetSpriteLayerVesselsEnabled(Entity spriteLayerEntity, bool enabled) {
            this.spriteLayerEntity = spriteLayerEntity;
            this.enabled = enabled;
        }
    }
}