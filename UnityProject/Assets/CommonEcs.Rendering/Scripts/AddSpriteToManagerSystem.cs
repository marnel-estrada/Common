using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    /// <summary>
    /// Adds sprites that are not yet added to the manager
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddSpriteToManagerSystem : ComponentSystem {
        // Note here that we're not using a common Added component so that each manager knows what 
        // sprite to remove
        public struct Added : ISystemStateComponentData {
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
        
        private EntityQuery query;
        
        private ArchetypeChunkComponentType<Sprite> spriteType;
        
        [ReadOnly]
        private ArchetypeChunkEntityType entityType;
        
        [ReadOnly]
        private ArchetypeChunkComponentType<LocalToWorld> matrixType;

        private SpriteManagerInstancesSystem spriteManagers;

        protected override void OnCreateManager() {
            // Note here that we filter sprites that doesn't have a SpriteManager added to them
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Added), typeof(SpriteManager)
            }, new ComponentType[] {
                typeof(LocalToWorld), typeof(Sprite)
            }));

            this.spriteManagers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.entityType = GetArchetypeChunkEntityType();
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            for (int i = 0; i < chunks.Length; ++i) {
                ProcessChunk(chunks[i]);
            }
            
            chunks.Dispose();
        }
        
        private void ProcessChunk(ArchetypeChunk chunk) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
            NativeArray<LocalToWorld> matrices = chunk.GetNativeArray(this.matrixType);

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
                Added added = new Added(sprite.spriteManagerEntity, sprite.managerIndex);
                this.PostUpdateCommands.AddComponent(entities[i], added);
                
                // We add the shared component so that it can be filtered using such shared component
                // in other systems. For example, in SortRenderOrderSystem.
                this.PostUpdateCommands.AddSharedComponent(entities[i], maybeManager.Value);
            }
        }

    }
}