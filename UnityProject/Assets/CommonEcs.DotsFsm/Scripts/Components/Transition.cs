using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    [InternalBufferCapacity(10)]
    public readonly struct Transition : IBufferElementData {
        // The event that would cause the transition
        public readonly FsmEvent fsmEvent;

        public readonly Entity fromState;
        public readonly Entity toState;

        public Transition(in Entity fromState, in FsmEvent fsmEvent, in Entity toState) {
            this.fsmEvent = fsmEvent;
            this.fromState = fromState;
            this.toState = toState;
        }
    }
}