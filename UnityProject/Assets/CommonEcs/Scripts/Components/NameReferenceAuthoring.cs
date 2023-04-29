using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// For most cases, names might not be added unto the components
    /// Instead, an entity will have a NameReference that will point to an entity with the
    /// Name component. We did it this way to avoid filling up the chunk when names are added
    /// to entities. 
    /// </summary>
    public struct NameReference : IComponentData {
        public NonNullEntity nameEntity;

        public NameReference(Entity nameEntity) {
            this.nameEntity = nameEntity;
        }
    }

    public class NameReferenceAuthoring : MonoBehaviour {
        // Only added in authoring to add the component
        // No editable values
        internal class Baker : SingleComponentBaker<NameReferenceAuthoring, NameReference> {
        }
    }
}