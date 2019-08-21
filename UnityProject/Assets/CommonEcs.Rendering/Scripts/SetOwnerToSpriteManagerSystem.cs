using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that sets the owner to each SpriteManager
    /// We did this because in SpriteLayer, we are creating a SpriteManager through a command buffer
    /// in which we can't get an entity during creation.
    /// </summary>
    [UpdateBefore(typeof(AddSpriteManagerToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SetOwnerToSpriteManagerSystem : ComponentSystem {
        private EntityQuery query;
        
        [ReadOnly]
        private ArchetypeChunkEntityType entityType;

        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        
        private struct OwnerSet : ISystemStateComponentData {
        }

        protected override void OnCreateManager() {
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
            this.entityType = GetArchetypeChunkEntityType();
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            for (int i = 0; i < chunks.Length; ++i) {
                ArchetypeChunk chunk = chunks[i];
                Process(ref chunk);
            }
            
            chunks.Dispose();
        }

        private void Process(ref ArchetypeChunk chunk) {
            SpriteManager spriteManager = this.spriteManagerQuery.GetSharedComponent(ref chunk);
            Assertion.Assert(spriteManager.HasInternalInstance);
            
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

            if (chunk.Count > 0) {
                if (spriteManager.Owner == Entity.Null) {
                    // Not set yet. We set it now.
                    spriteManager.Owner = entities[0];
                }

                // We add this component so that the entity won't be processed again
                this.PostUpdateCommands.AddComponent(entities[0], new OwnerSet());
            }
        }       
    }
}