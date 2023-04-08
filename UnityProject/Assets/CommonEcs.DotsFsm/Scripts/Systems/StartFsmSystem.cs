using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    public partial class StartFsmSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsm));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            StartFsmJob startFsmJob = new StartFsmJob() {
                fsmType = GetComponentTypeHandle<DotsFsm>()
            };

            return startFsmJob.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct StartFsmJob : IJobChunk {
            public ComponentTypeHandle<DotsFsm> fsmType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<DotsFsm> fsms = chunk.GetNativeArray(ref this.fsmType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    DotsFsm fsm = fsms[i];
                    if (fsm.startState.IsNone) {
                        // No start state
                        continue;
                    }
                    
                    // At this point, there's a start state
                    fsm.currentState = fsm.startState;
                    fsm.startState = ValueTypeOption<Entity>.None; // Clear so it won't be set again
                    
                    // Modify
                    fsms[i] = fsm;
                }
            }
        }
    }
}