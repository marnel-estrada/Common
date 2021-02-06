using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// An FSM state is an entity with this component and a dynamic buffer of entities that
    /// points to its actions
    /// </summary>
    public struct DotsFsmState : IComponentData {
        public readonly Entity fsmOwner;

        public DotsFsmState(Entity fsmOwner) {
            this.fsmOwner = fsmOwner;
        }
    }
}