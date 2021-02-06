using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    [InternalBufferCapacity(10)]
    public readonly struct Transition : IBufferElementData {
        // The event that would cause the transition
        public readonly FixedString64 eventId;

        public readonly Entity fromState;
        public readonly Entity toState;

        public Transition(Entity fromState, FixedString64 eventId, Entity toState) {
            this.eventId = eventId;
            this.fromState = fromState;
            this.toState = toState;
        }
    }
}