using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// For most cases, names might not be added unto the components
    /// Instead, an entity will have a NameReference that will point to an entity with the
    /// Name component. We did it this way to avoid filling up the chunk when names are added
    /// to entities.
    ///
    /// Components with GenerateAuthoringComponent cannot be readonly structs as it produces errors
    /// on its generated code. 
    /// </summary>
    [GenerateAuthoringComponent]
    public struct NameReference : IComponentData {
        public Entity nameEntity;

        public NameReference(Entity nameEntity) {
            this.nameEntity = nameEntity;
        }
    }
}