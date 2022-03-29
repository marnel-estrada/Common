using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    public partial class IdentifyRunningActionsSystem : SystemBase {
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmAction));
        }

        protected override void OnUpdate() {
            Job job = new Job() {
                actionType = GetComponentTypeHandle<DotsFsmAction>(), allFsms = GetComponentDataFromEntity<DotsFsm>()
            };

            this.Dependency = job.ScheduleParallel(this.query, this.Dependency);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<DotsFsmAction> actionType;

            [ReadOnly]
            public ComponentDataFromEntity<DotsFsm> allFsms;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsmAction> actions = batchInChunk.GetNativeArray(this.actionType);
                for (int i = 0; i < actions.Length; ++i) {
                    DotsFsmAction action = actions[i];
                    DotsFsm fsm = this.allFsms[action.fsmEntity];
                    
                    // Action can run if the FSM's current state is the action's state owner and
                    // the fsm has no pending event
                    // We no longer run the action if the FSM has a pending event
                    action.running = fsm.currentState.ValueOr(default) == action.stateOwner && fsm.pendingEvent.IsNone;

                    if (action.running) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    actions[i] = action; // Modify
                }
            }
        }
    }
}