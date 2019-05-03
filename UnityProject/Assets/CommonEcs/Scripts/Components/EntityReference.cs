using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to keep track of referencing of entities
    /// A system will use this data to remove referenced entities when the owner is already destroyed
    /// </summary>
    public struct EntityReference : IComponentData {
        public readonly Entity owner;

        public EntityReference(Entity owner) {
            this.owner = owner;
        }

        /// <summary>
        /// Create an entity with the reference relationship using an EntityManager
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="referred"></param>
        /// <param name="entityManager"></param>
        public static void Create(Entity owner, Entity referred, EntityManager entityManager) {
            entityManager.AddComponentData(referred, new EntityReference(owner));
        }

        /// <summary>
        /// Create an entity with the reference relationship using an EntityCommandBuffer
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="referred"></param>
        /// <param name="commandBuffer"></param>
        public static void Create(Entity owner, Entity referred, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(referred, new EntityReference(owner));
        }
    }
}