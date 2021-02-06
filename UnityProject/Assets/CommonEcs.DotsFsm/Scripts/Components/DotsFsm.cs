using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// An FSM is an entity with this component and a dynamic buffer of DotsFsmTransition
    /// elements.
    /// </summary>
    public struct DotsFsm : IComponentData {
        public ValueTypeOption<FixedString64> pendingEvent;
        public ValueTypeOption<Entity> currentState;

        public void SendEvent(FixedString64 eventId) {
            this.pendingEvent = ValueTypeOption<FixedString64>.Some(eventId);
        }
    }
}