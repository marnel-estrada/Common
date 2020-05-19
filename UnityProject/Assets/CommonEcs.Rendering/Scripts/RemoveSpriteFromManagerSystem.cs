using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RemoveSpriteFromManagerSystem : ComponentSystem {
        private EntityQuery query;

        private ArchetypeChunkComponentType<Sprite> spriteType;
        private ArchetypeChunkEntityType entityType;

        private SpriteManagerInstancesSystem spriteManagers;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Sprite), typeof(ForRemoval));
            this.spriteManagers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.entityType = GetArchetypeChunkEntityType();
            
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                ArchetypeChunk chunk = chunks[i];
                Process(ref chunk);
            }
            
            chunks.Dispose();
        }

        private void Process(ref ArchetypeChunk chunk) {
            NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            
            for (int i = 0; i < sprites.Length; ++i) {
                Sprite sprite = sprites[i];
                Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(sprite.spriteManagerEntity);
                Assertion.IsTrue(maybeManager.HasValue);
                maybeManager.Value.Remove(sprite);
            
                // Entities with ForRemoval means that the entity will be removed
                this.PostUpdateCommands.DestroyEntity(entities[i]);
            }
        }        
    }
}
