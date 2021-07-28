using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public struct DotsFsmAction : IComponentData {
        // Denormalized from DotsFsm so we don't need the DotsFsm instance
        // if we want to send an event.
        // There will be a system that will consume this value.
        public ValueTypeOption<FsmEvent> pendingEvent;

        public readonly Entity fsmEntity;
        public readonly Entity stateOwner;

        public bool running;
        public bool entered;
        public bool exited;

        public DotsFsmAction(Entity fsmEntity, Entity stateOwner) : this() {
            this.fsmEntity = fsmEntity;
            this.stateOwner = stateOwner;
        }

        public void SendEvent(in FsmEvent fsmEvent) {
            this.pendingEvent = ValueTypeOption<FsmEvent>.Some(fsmEvent);
        }
        
        public void SendEvent(in FixedString64 eventAsString) {
            if (eventAsString.Length == 0) {
                throw new Exception("Can't send an empty event.");
            }
            
            SendEvent(new FsmEvent(eventAsString));
        }
    }
}