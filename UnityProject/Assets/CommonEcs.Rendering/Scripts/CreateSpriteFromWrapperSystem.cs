using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CreateSpriteFromWrapperSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem barrier;
        
        private struct Created : ISystemStateComponentData {
        }

        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.barrier.CreateCommandBuffer();
            
            this.Entities.WithNone<Created>().ForEach(delegate(Entity entity, SpriteWrapper wrapper) {
                Sprite sprite = wrapper.Sprite;
                sprite.Init(wrapper.SpriteManagerEntity, sprite.width, sprite.height,
                    wrapper.pivot);
                commandBuffer.AddComponent(entity, sprite);

                if (wrapper.gameObject.isStatic) {
                    // Add static if it is static so it will not be added for transformation
                    commandBuffer.AddComponent(entity, new Static());
                }

                // We add this component so it will no longer be processed by this system
                commandBuffer.AddComponent(entity, new Created());
            }).WithoutBurst().Run();
        }
    }
}
