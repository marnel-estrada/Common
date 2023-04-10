using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that sets the owner to each SpriteManager
    /// We did this way because in SpriteLayer, we are creating a SpriteManager through a command buffer
    /// in which we can't get an entity during creation.
    /// </summary>
    [UpdateBefore(typeof(AddSpriteManagerToLayerSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class SetOwnerToSpriteManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
    
        private EntityQuery query;
        
        [ReadOnly]
        private EntityTypeHandle entityType;

        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        
        private struct OwnerSet : IComponentData {
        }

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
            
            // We added sprite for subtractive here because we only want to process those manager entities
            // and not the sprite entities where the SpriteManager is added as shared component
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(OwnerSet),
                typeof(Sprite)
            }, new ComponentType[] {
                typeof(SpriteManager)
            }));
            
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
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
            SpriteManager spriteManager = this.spriteManagerQuery.GetSharedComponent(ref chunk);
            Assertion.IsTrue(spriteManager.HasInternalInstance);
            
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

            if (chunk.Count > 0) {
                if (spriteManager.Owner == Entity.Null) {
                    // Not set yet. We set it now.
                    spriteManager.Owner = entities[0];
                }

                // We add this component so that the entity won't be processed again
                commandBuffer.AddComponent(entities[0], new OwnerSet());
            }
        }       
    }
}