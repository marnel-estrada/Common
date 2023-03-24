using Common;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateAfter(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RemoveGameObjectSpriteFromLayerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private EntityQuery query;
        
        private ComponentTypeHandle<AddGameObjectSpriteToLayerSystem.Added> addedType;
        private EntityTypeHandle entityType;
        
        private SpriteManagerInstancesSystem managersSystem;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
            
            this.query = GetEntityQuery(
                ComponentType.ReadOnly<AddGameObjectSpriteToLayerSystem.Added>(),
                ComponentType.Exclude<Transform>(),
                ComponentType.Exclude<Sprite>()
            );

            this.managersSystem = this.World.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.addedType = GetComponentTypeHandle<AddGameObjectSpriteToLayerSystem.Added>();
            this.entityType = GetEntityTypeHandle();
            
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i], ref commandBuffer);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk, ref EntityCommandBuffer commandBuffer) {
            NativeArray<AddGameObjectSpriteToLayerSystem.Added> addedList = chunk.GetNativeArray(ref this.addedType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            
            for (int i = 0; i < addedList.Length; ++i) {
                AddGameObjectSpriteToLayerSystem.Added added = addedList[i];
                Maybe<SpriteManager> manager = this.managersSystem.Get(added.spriteManagerEntity);
                Assertion.IsTrue(manager.HasValue);
                manager.Value.Remove(added.managerIndex);
                
                // We remove this component so it will no longer be processed by this system
                commandBuffer.RemoveComponent<AddGameObjectSpriteToLayerSystem.Added>(entities[i]);
            }   
        }        
    }
}
