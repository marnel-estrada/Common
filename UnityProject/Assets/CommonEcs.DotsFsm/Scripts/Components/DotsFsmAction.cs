using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public struct DotsFsmAction : IComponentData {
        // Denormalized from DotsFsm so we don't need the DotsFsm instance
        // if we want to send an event.
        // There will be a system that will consume this value.
        public ValueTypeOption<FixedString64> pendingEvent;

        public readonly Entity fsmOwner;
        public readonly Entity stateOwner;

        public bool running;
        public bool entered;
        public bool exited;

        public DotsFsmAction(Entity fsmOwner, Entity stateOwner) : this() {
            this.fsmOwner = fsmOwner;
            this.stateOwner = stateOwner;
        }
        
        public void SendEvent(FixedString64 eventId) {
            if (eventId.Length == 0) {
                throw new Exception("Can't send an empty event.");
            }
            
            this.pendingEvent = ValueTypeOption<FixedString64>.Some(eventId);
        }
    }
}