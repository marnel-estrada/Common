using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    /// <summary>
    /// Adds sprites that are not yet added to the manager
    /// </summary>
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class AddSpriteToManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private EntityQuery query;
        
        private ComponentTypeHandle<Sprite> spriteType;
        
        [ReadOnly]
        private EntityTypeHandle entityType;
        
        [ReadOnly]
        private ComponentTypeHandle<LocalToWorld> matrixType;

        private SpriteManagerInstancesSystem spriteManagers;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            
            // Note here that we filter sprites that doesn't have a SpriteManager added to them
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Added), typeof(SpriteManager), typeof(AddToSpriteLayer)
            }, new ComponentType[] {
                typeof(LocalToWorld), typeof(Sprite)
            }));

            this.spriteManagers = this.World.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.spriteType = GetComponentTypeHandle<Sprite>();
            this.matrixType = GetComponentTypeHandle<LocalToWorld>(true);
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);

            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            
            for (int i = 0; i < chunks.Length; ++i) {
                ProcessChunk(chunks[i], ref commandBuffer);
            }
            
            chunks.Dispose();
        }
        
        private void ProcessChunk(ArchetypeChunk chunk, ref EntityCommandBuffer commandBuffer) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
            NativeArray<LocalToWorld> matrices = chunk.GetNativeArray(ref this.matrixType);

            for (int i = 0; i < chunk.Count; ++i) {
                Sprite sprite = sprites[i];
                if (sprite.spriteManagerEntity == Entity.Null) {
                    continue;
                }
            
                LocalToWorld transform = matrices[i];
                Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(sprite.spriteManagerEntity);
                maybeManager.Value.Add(ref sprite, transform.Value);
                sprites[i] = sprite; // Modify the sprite data

                // Add this component so it will no longer be processed by this system
                Added added = new(sprite.spriteManagerEntity, sprite.managerIndex);
                commandBuffer.AddComponent(entities[i], added);
                
                // We add the shared component so that it can be filtered using such shared component
                // in other systems. For example, in SortRenderOrderSystem.
                commandBuffer.AddSharedComponentManaged(entities[i], maybeManager.Value);
            }
        }
        
        // Note here that we're not using a common Added component so that each manager knows what 
        // sprite to remove
        public readonly struct Added : ICleanupComponentData {
            // The entity of the sprite manager to where the sprite is added
            public readonly Entity spriteManagerEntity;
        
            // This is the managerIndex of the sprite so we can determine what index they are if they were somehow
            // removed
            public readonly int managerIndex;

            public Added(Entity spriteManagerEntity, int managerIndex) {
                this.spriteManagerEntity = spriteManagerEntity;
                this.managerIndex = managerIndex;
            }
        }
    }
}