using Unity.Entities;
using UnityEngine;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// A component that can be added to entities that requires reference to a utility brain entity.
    /// </summary>
    //[GenerateAuthoringComponent]
    public struct UtilityBrainReference : IComponentData {
        public NonNullEntity utilityBrainEntity;

        public UtilityBrainReference(Entity utilityBrainEntity) {
            this.utilityBrainEntity = utilityBrainEntity;
        }
    }

    public class UtilityBrainReferenceAuthoring : MonoBehaviour {
        // Only added in authoring to add the component
        // No editable values
    }
}