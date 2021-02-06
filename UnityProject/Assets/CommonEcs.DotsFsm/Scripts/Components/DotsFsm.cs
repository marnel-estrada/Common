using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// An FSM is an entity with this component and a dynamic buffer of DotsFsmTransition
    /// elements.
    /// </summary>
    public struct DotsFsm : IComponentData {
        public ValueTypeOption<Entity> currentState;
    }
}