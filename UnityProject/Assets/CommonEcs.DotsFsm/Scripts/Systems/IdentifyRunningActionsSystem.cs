using Unity.Entities;

namespace CommonEcs.DotsFsm {
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    public class IdentifyRunningActionsSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.ForEach(delegate(ref DotsFsmAction action) {
                DotsFsmState state = GetComponent<DotsFsmState>(action.stateOwner);
                DotsFsm fsm = GetComponent<DotsFsm>(state.fsmOwner);

                // Action can run if the FSM's current state is the action's state owner and
                // the fsm has no pending event
                // We no longer run the action if the FSM has a pending event
                action.running = fsm.currentState.ValueOr(default) == action.stateOwner && fsm.pendingEvent.IsNone;
            }).ScheduleParallel();
        }
    }
}