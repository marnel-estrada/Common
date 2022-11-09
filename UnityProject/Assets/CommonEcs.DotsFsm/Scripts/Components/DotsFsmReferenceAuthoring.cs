using Unity.Entities;

using UnityEngine;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A utility component that stores a reference to an FSM entity
    /// </summary>
    // [GenerateAuthoringComponent]
    public struct DotsFsmReference : IComponentData {
        public NonNullEntity fsmEntity;

        public DotsFsmReference(Entity fsmEntity) {
            this.fsmEntity = fsmEntity;
        }
    }

    public class DotsFsmReferenceAuthoring : MonoBehaviour {
    }
}