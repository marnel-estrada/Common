using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TimedWaitActionSystem : FsmActionSystem {
        private ComponentTypeHandle<TimedWaitAction> waitType;
        private ComponentTypeHandle<DurationTimer> timerType;
        
        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(FsmAction), typeof(TimedWaitAction), typeof(DurationTimer));
        }

        protected override void BeforeChunkTraversal() {
            base.BeforeChunkTraversal();

            this.waitType = GetComponentTypeHandle<TimedWaitAction>();
            this.timerType = GetComponentTypeHandle<DurationTimer>();
        }

        private NativeArray<TimedWaitAction> waitActions;
        private NativeArray<DurationTimer> timers;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            base.BeforeProcessChunk(chunk);

            this.waitActions = chunk.GetNativeArray(this.waitType);
            this.timers = chunk.GetNativeArray(this.timerType);
        }

        protected override void DoEnter(int index, ref FsmAction fsmAction) {
            TimedWaitAction waitAction = this.waitActions[index];

            if(Comparison.IsZero(waitAction.duration)) {
                // No need to wait as duration is zero
                Finish(index, ref fsmAction);
                return;
            }

            DurationTimer timer = this.timers[index];
            timer.Reset(waitAction.duration);
            this.timers[index] = timer; // Modify
        }

        protected override void DoUpdate(int index, ref FsmAction fsmAction) {
            DurationTimer timer = this.timers[index];

            if (timer.HasElapsed) {
                // Timed duration has already elapsed. Time to end the action.
                Finish(index, ref fsmAction);
            }
        }

        private void Finish(int index, ref FsmAction fsmAction) {
            fsmAction.finished = true;

            // Send finish event
            TimedWaitAction waitAction = this.waitActions[index];
            if(waitAction.finishEvent != Fsm.NULL_EVENT) {
                // Has a finish event
                SendEvent(ref fsmAction, waitAction.finishEvent);
            }

            // Destroy so that it will no longer be processed
            this.PostUpdateCommands.DestroyEntity(GetEntityAt(index));
        }
    }
}
