using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A utility component that stores a reference to an FSM entity
    /// </summary>
    [GenerateAuthoringComponent]
    public struct DotsFsmReference : IComponentData {
        public Entity fsmEntity;

        public DotsFsmReference(Entity fsmEntity) {
            this.fsmEntity = fsmEntity;
        }
    }
}