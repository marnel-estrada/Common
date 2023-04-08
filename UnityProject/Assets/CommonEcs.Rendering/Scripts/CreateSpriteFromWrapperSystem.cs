using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class CreateSpriteFromWrapperSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            
            this.Entities.WithoutBurst().WithNone<Created>().ForEach((Entity entity, SpriteWrapper wrapper) => {
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
            }).Run();
        }
        
        private struct Created : IComponentData {
        }
    }
}
