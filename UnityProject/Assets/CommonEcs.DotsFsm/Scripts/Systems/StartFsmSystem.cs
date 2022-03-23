using Unity.Burst;
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
            Job job = new Job() {
                fsmType = GetComponentTypeHandle<DotsFsm>()
            };

            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<DotsFsm> fsmType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsm> fsms = batchInChunk.GetNativeArray(this.fsmType);
                for (int i = 0; i < fsms.Length; ++i) {
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