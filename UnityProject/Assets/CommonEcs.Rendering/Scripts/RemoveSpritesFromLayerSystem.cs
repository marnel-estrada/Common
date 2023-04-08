using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateAfter(typeof(AddSpritesToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RemoveSpritesFromLayerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private EntityQuery query;
        private ComponentTypeHandle<AddSpritesToLayerSystem.Added> addedType;
        private EntityTypeHandle entityType;
        
        private SpriteManagerInstancesSystem managers;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
            this.managers = this.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();

            this.query = GetEntityQuery(ComponentType.ReadOnly<AddSpritesToLayerSystem.Added>(),
                ComponentType.Exclude<Sprite>());
        }

        protected override void OnUpdate() {
            this.addedType = GetComponentTypeHandle<AddSpritesToLayerSystem.Added>();
            this.entityType = GetEntityTypeHandle();

            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i], ref commandBuffer);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk, ref EntityCommandBuffer commandBuffer) {
            NativeArray<AddSpritesToLayerSystem.Added> addedList = chunk.GetNativeArray(ref this.addedType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                AddSpritesToLayerSystem.Added added = addedList[i];
                Maybe<SpriteManager> manager = this.managers.Get(added.spriteManagerEntity);
                Assertion.IsTrue(manager.HasValue);
                manager.Value.Remove(added.managerIndex);
                
                // We remove this component so it will no longer be processed by this system
                commandBuffer.RemoveComponent<AddSpritesToLayerSystem.Added>(entities[i]);
            }
        }
    }
}