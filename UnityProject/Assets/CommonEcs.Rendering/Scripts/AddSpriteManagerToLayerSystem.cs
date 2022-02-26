using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddSpritesToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddSpriteManagerToLayerSystem : ComponentSystem {
        private struct Processed : IComponentData {
        }

        private EntityQuery query;
        private SharedComponentTypeHandle<SpriteManager> spriteManagerType;

        private SpriteLayerInstancesSystem layers;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(SpriteManager), ComponentType.Exclude<Processed>(), 
                ComponentType.Exclude<Sprite>());
            this.layers = this.World.GetExistingSystem<SpriteLayerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.spriteManagerType = GetSharedComponentTypeHandle<SpriteManager>();
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }
        
        private void Process(ArchetypeChunk chunk) {
            SpriteManager manager = chunk.GetSharedComponentData(this.spriteManagerType, this.EntityManager);
            if (manager.SpriteLayerEntity != Entity.Null) {
                // There's an assigned layer. We add the manager to such layer.
                Maybe<SpriteLayer> result = this.layers.Get(manager.SpriteLayerEntity);
                if (result.HasValue) {
                    result.Value.spriteManagerEntities.Add(manager.Owner);
                }
            }
            
            // We add this component so it will be skipped on the next frame
            this.PostUpdateCommands.AddComponent(manager.Owner, new Processed());
        }
    }
}
