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
    public class RemoveSpritesFromLayerSystem : ComponentSystem {
        private EntityQuery query;
        private ComponentTypeHandle<AddSpritesToLayerSystem.Added> addedType;
        private EntityTypeHandle entityType;
        
        private SpriteManagerInstancesSystem managers;

        protected override void OnCreate() {
            this.managers = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpriteManagerInstancesSystem>();

            this.query = GetEntityQuery(ComponentType.ReadOnly<AddSpritesToLayerSystem.Added>(),
                ComponentType.Exclude<Sprite>());
        }

        protected override void OnUpdate() {
            this.addedType = GetComponentTypeHandle<AddSpritesToLayerSystem.Added>();
            this.entityType = GetEntityTypeHandle();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk) {
            NativeArray<AddSpritesToLayerSystem.Added> addedList = chunk.GetNativeArray(this.addedType);
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                AddSpritesToLayerSystem.Added added = addedList[i];
                Maybe<SpriteManager> manager = this.managers.Get(added.spriteManagerEntity);
                Assertion.IsTrue(manager.HasValue);
                manager.Value.Remove(added.managerIndex);
                
                // We remove this component so it will no longer be processed by this system
                this.PostUpdateCommands.RemoveComponent<AddSpritesToLayerSystem.Added>(entities[i]);
            }
        }
    }
}