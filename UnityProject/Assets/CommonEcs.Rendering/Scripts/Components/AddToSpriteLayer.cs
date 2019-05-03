namespace CommonEcs {
    using Unity.Entities;
    
    /// <summary>
    /// A component that identifies a certain sprite to add to sprite layer
    /// </summary>
    public struct AddToSpriteLayer : IComponentData {
        public Entity layerEntity;

        public AddToSpriteLayer(Entity layerEntity) {
            this.layerEntity = layerEntity;
        }
    }
}
