using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public readonly struct SendEvent : IComponentData {
        public readonly Entity fsmEntity;
        public readonly FixedString64 eventId;

        public SendEvent(Entity fsmEntity, FixedString64 eventId) {
            this.fsmEntity = fsmEntity;
            this.eventId = eventId;
        }
    }
}