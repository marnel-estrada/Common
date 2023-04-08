using CommonEcs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// This is just a gating system such that action systems can determine when they can execute
    /// </summary>
    public partial class FsmActionEndSystem : JobSystemBase {
        private EndInitializationEntityCommandBufferSystem barrier;

        private EntityQuery query;

        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();

            this.query = GetEntityQuery(typeof(FsmAction));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            DestroyFinishedActionsJob job = new() {
                entityType = GetEntityTypeHandle(),
                actionType = GetComponentTypeHandle<FsmAction>(),
                commandBuffer = this.barrier.CreateCommandBuffer().AsParallelWriter()
            };
            JobHandle handle = job.ScheduleParallel(this.query, inputDeps);
            this.barrier.AddJobHandleForProducer(handle);
        
            return handle;
        }
        
        [BurstCompile]
        private struct DestroyFinishedActionsJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;

            [ReadOnly]
            public ComponentTypeHandle<FsmAction> actionType;
            
            public EntityCommandBuffer.ParallelWriter commandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<FsmAction> actions = chunk.GetNativeArray(this.actionType);

                for (int i = 0; i < chunk.Count; i++) {
                    FsmAction action = actions[i];
                    if (action.finished) {
                        // Action is finished. Time to destroy.
                        int sortKey = firstEntityIndex + i;
                        this.commandBuffer.DestroyEntity(sortKey, entities[i]);
                    }
                }
            }
        }
    }
}
