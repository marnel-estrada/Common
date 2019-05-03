using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A struct that wraps an Entity for use in DynamicBuffer
    /// </summary>
    [InternalBufferCapacity(10)]
    public struct EntityBufferElement : IBufferElementData {
        public Entity entity;

        public EntityBufferElement(Entity entity) {
            this.entity = entity;
        }
    }
}