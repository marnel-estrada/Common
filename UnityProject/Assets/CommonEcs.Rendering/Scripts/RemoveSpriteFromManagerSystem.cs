using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class RemoveSpriteFromManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private EntityQuery query;

        private ComponentTypeHandle<Sprite> spriteType;
        private EntityTypeHandle entityType;

        private SpriteManagerInstancesSystem spriteManagers;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
            
            this.query = GetEntityQuery(typeof(Sprite), typeof(ForRemoval));
            this.spriteManagers = this.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.spriteType = GetComponentTypeHandle<Sprite>();
            this.entityType = GetEntityTypeHandle();
            
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            for (int i = 0; i < chunks.Length; ++i) {
                ArchetypeChunk chunk = chunks[i];
                Process(ref chunk, ref commandBuffer);
            }
            
            chunks.Dispose();
        }

        private void Process(ref ArchetypeChunk chunk, ref EntityCommandBuffer commandBuffer) {
            NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            
            for (int i = 0; i < sprites.Length; ++i) {
                Sprite sprite = sprites[i];
                Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(sprite.spriteManagerEntity);
                Assertion.IsTrue(maybeManager.HasValue);
                maybeManager.Value.Remove(sprite);
            
                // Entities with ForRemoval means that the entity will be removed
                commandBuffer.DestroyEntity(entities[i]);
            }
        }        
    }
}
