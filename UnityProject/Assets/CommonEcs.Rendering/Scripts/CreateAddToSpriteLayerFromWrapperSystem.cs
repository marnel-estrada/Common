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
    public class CreateAddToSpriteLayerFromWrapperSystem : TemplateComponentSystem {
        private struct Created : ISystemStateComponentData {
        }

        private ArchetypeChunkEntityType entityType;
        private ArchetypeChunkComponentType<AddToSpriteLayerWrapper> wrapperType;

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(AddToSpriteLayerWrapper), ComponentType.Exclude<Created>());
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetArchetypeChunkEntityType();
            this.wrapperType = GetArchetypeChunkComponentType<AddToSpriteLayerWrapper>();
        }

        private NativeArray<Entity> entities;
        private ArchetypeChunkComponentObjects<AddToSpriteLayerWrapper> wrappers;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.wrappers = chunk.GetComponentObjects<AddToSpriteLayerWrapper>(this.wrapperType, this.EntityManager);
        }

        protected override void Process(int index) {
            Entity entity = this.entities[index];
            AddToSpriteLayerWrapper wrapper = this.wrappers[index];
            this.PostUpdateCommands.AddComponent(entity, wrapper.componentData);
            
            // We add this component so it will no longer be processed by this system
            this.PostUpdateCommands.AddComponent(entity, new Created());
        }        
    }
}
