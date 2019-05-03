using Unity.Entities;

namespace Common.Ecs.Fsm {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmStatePreparationJobSystemBarrier : EntityCommandBufferSystem {    
    }
}
