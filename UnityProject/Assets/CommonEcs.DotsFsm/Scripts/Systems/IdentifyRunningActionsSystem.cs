using Unity.Burst;
using Unity.Burst.Intrinsics;
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
            Job job = new() {
                actionType = GetComponentTypeHandle<DotsFsmAction>(), 
                allFsms = GetComponentLookup<DotsFsm>()
            };

            this.Dependency = job.ScheduleParallel(this.query, this.Dependency);
        }
        
        [BurstCompile]
        private struct Job : IJobChunk {
            public ComponentTypeHandle<DotsFsmAction> actionType;

            [ReadOnly]
            public ComponentLookup<DotsFsm> allFsms;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<DotsFsmAction> actions = chunk.GetNativeArray(ref this.actionType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
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