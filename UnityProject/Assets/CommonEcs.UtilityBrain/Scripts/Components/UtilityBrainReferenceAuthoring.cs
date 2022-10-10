using Unity.Entities;

using UnityEngine;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// A component that can be added to entities that requires reference to a utility brain entity.
    /// </summary>
    public struct UtilityBrainReference : IComponentData {
        public NonNullEntity utilityBrainEntity;

        public UtilityBrainReference(Entity utilityBrainEntity) {
            this.utilityBrainEntity = utilityBrainEntity;
        }
    }

    public class UtilityBrainReferenceAuthoring : MonoBehaviour {
        public NonNullEntity utilityBrainEntity;
    }
}