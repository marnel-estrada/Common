using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateBefore(typeof(AddSpritesToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class AddSpriteManagerToLayerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;

        private EntityQuery query;
        private SharedComponentTypeHandle<SpriteManager> spriteManagerType;

        private SpriteLayerInstancesSystem layers;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
            
            this.query = GetEntityQuery(typeof(SpriteManager), ComponentType.Exclude<Processed>(), 
                ComponentType.Exclude<Sprite>());
            
            this.layers = this.World.GetOrCreateSystemManaged<SpriteLayerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.spriteManagerType = GetSharedComponentTypeHandle<SpriteManager>();
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);

            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i], ref commandBuffer);
            }
            
            chunks.Dispose();
        }
        
        private void Process(ArchetypeChunk chunk, ref EntityCommandBuffer commandBuffer) {
            SpriteManager manager = chunk.GetSharedComponentManaged(this.spriteManagerType, this.EntityManager);
            if (manager.SpriteLayerEntity != Entity.Null) {
                // There's an assigned layer. We add the manager to such layer.
                Maybe<SpriteLayer> result = this.layers.Get(manager.SpriteLayerEntity);
                if (result.HasValue) {
                    result.Value.spriteManagerEntities.Add(manager.Owner);
                }
            }
            
            // We add this component so it will be skipped on the next frame
            commandBuffer.AddComponent(manager.Owner, new Processed());
        }
        
        private struct Processed : IComponentData {
        }
    }
}
