using System;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// This is just a gating system such that action systems can determine when they can execute
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmActionStartSystem : ComponentSystem {
        protected override void OnUpdate() {
        }
    }
}
