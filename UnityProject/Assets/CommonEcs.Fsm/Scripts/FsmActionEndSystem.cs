using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// This is just a gating system such that action systems can determine when they can execute
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmActionEndSystem : JobComponentSystem {
        private EndPresentationEntityCommandBufferSystem barrier;

        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                commandBuffer = this.barrier.CreateCommandBuffer().ToConcurrent()
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            this.barrier.AddJobHandleForProducer(handle);
        
            return handle;
        }
        
        [BurstCompile]
        private struct Job : IJobForEachWithEntity<FsmAction> {
            public EntityCommandBuffer.Concurrent commandBuffer;
            
            public void Execute(Entity entity, int index, ref FsmAction action) {
                if (action.finished) {
                    // Action is finished. Time to destroy.
                    this.commandBuffer.DestroyEntity(index, entity);
                }
            }
        }
    }
}
