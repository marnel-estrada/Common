using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A utility component that stores a reference to an FSM entity
    /// </summary>
    [GenerateAuthoringComponent]
    public readonly struct DotsFsmReference : IComponentData {
        public readonly Entity fsmEntity;

        public DotsFsmReference(Entity fsmEntity) {
            this.fsmEntity = fsmEntity;
        }
    }
}