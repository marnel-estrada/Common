using Unity.Entities;

namespace Common.Ecs.Fsm {
    public struct FsmTransition : IBufferElementData {
        public Entity fsmOwner;

        public Entity fromState;
        public uint transitionEvent; // The event that would cause the transition
        public Entity toState;
    }
}
