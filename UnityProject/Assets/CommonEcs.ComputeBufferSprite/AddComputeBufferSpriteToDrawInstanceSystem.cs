using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateAfter(typeof(UpdateComputeBufferSpriteTransformSystem))]
    public class AddComputeBufferSpriteToDrawInstanceSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem barrier;
        
        // This is a collection of draw instances that can be referenced using their Entity
        private CollectDrawInstancesSystem drawInstances;

        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this.drawInstances = this.World.GetOrCreateSystem<CollectDrawInstancesSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.barrier.CreateCommandBuffer();
            
            this.Entities.WithNone<AddRegistry, ComputeBufferDrawInstance>().ForEach(
                delegate(Entity entity, ref ComputeBufferSprite sprite, ref Translation translation, ref NonUniformScale scale) {
                    if (sprite.drawInstanceEntity == Entity.Null) {
                        // No draw instance entity assigned. Can't get which draw instance this sprite is to
                        // add to.
                        return;
                    }

                    Maybe<ComputeBufferDrawInstance> drawInstance = this.drawInstances.Get(sprite.drawInstanceEntity);
                    
                    // Note here that we already set the sprite's transform prior to adding
                    sprite.SetTransform(translation.Value.xy, scale.Value.xy);
                    drawInstance.Value.Add(ref sprite);
                    
                    // Add this component so it will no longer be processed by this system
                    AddRegistry registry = new AddRegistry(sprite.drawInstanceEntity, sprite.masterListIndex);
                    commandBuffer.AddComponent(entity, registry);
                    
                    // We add the shared component so that it can be filtered using such shared component
                    // in other systems.
                    commandBuffer.AddSharedComponent(entity, drawInstance.Value);
                }).WithoutBurst().Run();
        }
    }
}