using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// For most cases, names might not be added unto the components
    /// Instead, an entity will have a NameReference that will point to an entity with the
    /// Name component. We did it this way to avoid filling up the chunk when names are added
    /// to entities.
    /// </summary>
    public readonly struct NameReference : IComponentData {
        public readonly Entity nameEntity;

        public NameReference(Entity nameEntity) {
            this.nameEntity = nameEntity;
        }
    }
}