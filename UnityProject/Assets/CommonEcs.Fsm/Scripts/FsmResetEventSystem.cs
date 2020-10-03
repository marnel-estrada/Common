using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace Common.Ecs.Fsm {

    /// <summary>
    /// Resets the event of the system in case they don't have transitions
    /// </summary>
    [UpdateAfter(typeof(FsmConsumeEventSystem))]
    [UpdateBefore(typeof(FsmActionStartSystem))]
    [UpdateAfter(typeof(FsmStatePreparationJobSystemBarrier))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmResetEventSystem : JobComponentSystem {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Fsm));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                fsmType = GetComponentTypeHandle<Fsm>()
            };

            return JobChunkExtensions.Schedule(job, this.query, inputDeps);
        }

        [BurstCompile]
        private struct Job : IJobChunk {
            public ComponentTypeHandle<Fsm> fsmType;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Fsm> fsms = chunk.GetNativeArray(this.fsmType);
                for (int i = 0; i < chunk.Count; ++i) {
                    Fsm fsm = fsms[i];
                    if (fsm.currentEvent != Fsm.NULL_EVENT) {
                        //Debug.Log("Fsm has no transition for current event: " + fsm.currentEvent);
                        fsm.currentEvent = Fsm.NULL_EVENT;
                        fsms[i] = fsm; // Update the data
                    }                    
                }
            }
        }
    }
}
