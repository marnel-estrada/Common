using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A component that identifies a certain sprite to add to sprite layer
    /// </summary>
    [GenerateAuthoringComponent]
    public struct AddToSpriteLayer : IComponentData {
        public Entity layerEntity;

        public AddToSpriteLayer(Entity layerEntity) {
            this.layerEntity = layerEntity;
        }
    }
}
