using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CreateSpriteFromWrapperSystem : TemplateComponentSystem {
        private struct Created : ISystemStateComponentData {
        }

        private ArchetypeChunkEntityType entityType;
        private ArchetypeChunkComponentType<SpriteWrapper> wrapperType;
        private ArchetypeChunkComponentType<Created> createdType;

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(SpriteWrapper), ComponentType.Exclude<Created>());
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetArchetypeChunkEntityType();
            this.wrapperType = GetArchetypeChunkComponentType<SpriteWrapper>();
            this.createdType = GetArchetypeChunkComponentType<Created>();
        }

        private NativeArray<Entity> entities;
        private ArchetypeChunkComponentObjects<SpriteWrapper> wrappers;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.wrappers = chunk.GetComponentObjects(this.wrapperType, this.EntityManager);
        }

        protected override void Process(int index) {
            Entity entity = this.entities[index];
            SpriteWrapper wrapper = this.wrappers[index];

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
        }
    }
}
