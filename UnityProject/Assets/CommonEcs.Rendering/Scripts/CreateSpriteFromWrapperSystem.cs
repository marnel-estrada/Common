using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CreateSpriteFromWrapperSystem : ComponentSystem {
        private EntityQuery query;
        
        private struct Created : IComponentData {
        }

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(SpriteWrapper), ComponentType.Exclude<Created>());
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach((Entity entity, SpriteWrapper wrapper) => {
                Sprite sprite = wrapper.Sprite;
                sprite.Init(wrapper.SpriteManagerEntity, sprite.width, sprite.height,
                    wrapper.pivot);
                this.PostUpdateCommands.AddComponent(entity, sprite);

                if (wrapper.gameObject.isStatic) {
                    // Add static if it is static so it will not be added for transformation
                    this.PostUpdateCommands.AddComponent(entity, new Static());
                }

                // We add this component so it will no longer be processed by this system
                this.PostUpdateCommands.AddComponent(entity, new Created());
            });
        }
    }
}
