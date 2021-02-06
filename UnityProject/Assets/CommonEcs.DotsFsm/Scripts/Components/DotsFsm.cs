using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.DotsFsm {
    /// <summary>
    /// An FSM is an entity with this component and a dynamic buffer of DotsFsmTransition
    /// elements.
    /// </summary>
    public struct DotsFsm : IComponentData {
        // This will be consumed by a system that handles the transitions
        public ValueTypeOption<FixedString64> sentEvent;
        
        public ValueTypeOption<Entity> currentState;
    }
}