using Unity.Entities;

namespace CommonEcs {
    public class RemoveDestroyedComputeBufferSpriteFromDrawInstanceSystem : SystemBase {
        // This is a collection of draw instances that can be referenced using their Entity
        private CollectDrawInstancesSystem drawInstances;
        private EndSimulationEntityCommandBufferSystem barrier;

        protected override void OnCreate() {
            this.drawInstances = this.World.GetOrCreateSystem<CollectDrawInstancesSystem>();
            this.barrier = this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.barrier.CreateCommandBuffer();
            
            this.Entities.WithNone<ComputeBufferSprite, ComputeBufferDrawInstance>().ForEach(delegate(Entity entity, ref AddRegistry registry) {
                Maybe<ComputeBufferDrawInstance> drawInstance = this.drawInstances.Get(registry.drawInstanceEntity);
                drawInstance.Value.Remove(registry.masterListIndex);
                
                // We remove this component so it will no longer be processed by this system
                commandBuffer.RemoveComponent<AddRegistry>(entity);
            }).WithoutBurst().Run();
        }
    }
}