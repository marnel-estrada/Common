using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// An FSM is an entity with this component and a dynamic buffer of DotsFsmTransition
    /// elements.
    /// </summary>
    public struct DotsFsm : IComponentData {
        // We don't use FixedString here to conserve archetype space
        public ValueTypeOption<FsmEvent> pendingEvent;
        public ValueTypeOption<Entity> currentState;

        public void SendEvent(in FsmEvent fsmEvent) {
            if (this.pendingEvent.IsSome) {
                // This means that there could be two actions that sent events
                throw new Exception("Can't replace existing pending event");
            }
            
            this.pendingEvent = ValueTypeOption<FsmEvent>.Some(fsmEvent);
        }

        public void SendEvent(in FixedString64 eventAsString) {
            if (eventAsString.Length == 0) {
                throw new Exception("Can't send empty events");
            }
            
            SendEvent(new FsmEvent(eventAsString));
        }
    }
}