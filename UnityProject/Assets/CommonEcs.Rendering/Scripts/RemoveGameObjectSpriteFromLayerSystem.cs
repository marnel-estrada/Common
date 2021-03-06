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
    public class RemoveGameObjectSpriteFromLayerSystem : ComponentSystem {
        private EntityQuery query;
        
        private ComponentTypeHandle<AddGameObjectSpriteToLayerSystem.Added> addedType;
        private EntityTypeHandle entityType;
        
        private SpriteManagerInstancesSystem managers;

        protected override void OnCreate() {
            this.query = GetEntityQuery(
                ComponentType.ReadOnly<AddGameObjectSpriteToLayerSystem.Added>(),
                ComponentType.Exclude<Transform>(),
                ComponentType.Exclude<Sprite>()
            );

            this.managers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.addedType = GetComponentTypeHandle<AddGameObjectSpriteToLayerSystem.Added>();
            this.entityType = GetEntityTypeHandle();
            
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk) {
            NativeArray<AddGameObjectSpriteToLayerSystem.Added> addedList = chunk.GetNativeArray(this.addedType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            
            for (int i = 0; i < addedList.Length; ++i) {
                AddGameObjectSpriteToLayerSystem.Added added = addedList[i];
                Maybe<SpriteManager> manager = this.managers.Get(added.spriteManagerEntity);
                Assertion.IsTrue(manager.HasValue);
                manager.Value.Remove(added.managerIndex);
                
                // We remove this component so it will no longer be processed by this system
                this.PostUpdateCommands.RemoveComponent<AddGameObjectSpriteToLayerSystem.Added>(entities[i]);
            }   
        }        
    }
}
