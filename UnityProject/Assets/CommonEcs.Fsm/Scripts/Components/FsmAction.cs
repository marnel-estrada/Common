using CommonEcs;

using Unity.Entities;

namespace Common.Ecs.Fsm {
    public struct FsmAction : IComponentData {
        public Entity stateOwner;
        public ByteBool entered;
        public ByteBool finished;

        // The index of this action
        public uint actionIndex;
    }
}
