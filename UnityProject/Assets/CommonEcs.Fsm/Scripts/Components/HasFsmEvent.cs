using Unity.Entities;

namespace Common.Ecs.Fsm {
    // This is a tag that indicates that an FSM has a pending (unconsumed) event
    public struct HasFsmEvent : IComponentData {
    }
}
