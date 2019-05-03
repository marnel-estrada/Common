using System;

using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// Common barrier for FsmActionJobSystem instances
    /// </summary>
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmActionJobSystemBarrier : EntityCommandBufferSystem {
    }
}
