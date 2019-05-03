using System;

using Unity.Entities;

namespace Common.Ecs.Fsm {
    public struct FsmState : IComponentData {

        public Entity entityOwner;

        public Entity fsmOwner;

        // This is used on state transition
        // This is ID mapped to an action or delegate that prepares the state's actions
        public byte stateId;

    }
}
