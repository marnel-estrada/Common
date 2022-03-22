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
    public class CreateAddToSpriteLayerFromWrapperSystem : ComponentSystem {
        private EntityQuery query;
        
        private struct Created : IComponentData {
        }

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AddToSpriteLayerWrapper), ComponentType.Exclude<Created>());
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach((Entity entity, AddToSpriteLayerWrapper wrapper) => {
                this.PostUpdateCommands.AddComponent(entity, wrapper.componentData);
            
                // We add this component so it will no longer be processed by this system
                this.PostUpdateCommands.AddComponent(entity, new Created());
            });
        }
    }
}
