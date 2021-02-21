using System;

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
            if (eventId.Length == 0) {
                throw new Exception("Can't send empty events");
            }

            if (this.pendingEvent.IsSome) {
                // This means that there could be two actions that sent events
                throw new Exception("Can't replace existing pending event");
            }
            
            this.pendingEvent = ValueTypeOption<FixedString64>.Some(eventId);
        }
    }
}