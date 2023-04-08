namespace CommonEcs {
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Adds the actual AddToSpriteLayer component to the GameObjectEntity
    /// </summary>
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateBefore(typeof(CreateSpriteFromWrapperSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial  class CreateAddToSpriteLayerFromWrapperSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            
            this.Entities.WithoutBurst().WithNone<Created>().ForEach((Entity entity, AddToSpriteLayerWrapper wrapper) => {
                commandBuffer.AddComponent(entity, wrapper.componentData);
            
                // We add this component so it will no longer be processed by this system
                commandBuffer.AddComponent(entity, new Created());
            }).Run();
        }
        
        private struct Created : IComponentData {
        }
    }
}
