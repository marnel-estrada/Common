using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using Common;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateBefore(typeof(AddGameObjectSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddGameObjectSpriteToLayerSystem : ComponentSystem {
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
        private ArchetypeChunkComponentType<AddToSpriteLayer> addToLayerType;
        private ArchetypeChunkComponentType<Transform> transformType;
        private ArchetypeChunkEntityType entityType;

        private SpriteLayerInstancesSystem layers;
        private SpriteManagerInstancesSystem managers;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(Transform), typeof(AddToSpriteLayer), typeof(Sprite)
            }, new ComponentType[] {
                typeof(Added)
            }));

            this.layers = this.World.GetOrCreateSystem<SpriteLayerInstancesSystem>();
            this.managers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.addToLayerType = GetArchetypeChunkComponentType<AddToSpriteLayer>();
            this.transformType = GetArchetypeChunkComponentType<Transform>();
            this.entityType = GetArchetypeChunkEntityType();
            
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                if (!Process(chunks[i])) {
                    break;
                }
            }
            
            chunks.Dispose();
        }

        private NativeArray<AddToSpriteLayer> addToLayers;
        private NativeArray<Sprite> sprites;
        private ArchetypeChunkComponentObjects<Transform> transforms;
        private NativeArray<Entity> entities;

        private bool Process(ArchetypeChunk chunk) {
            this.addToLayers = chunk.GetNativeArray(this.addToLayerType);
            this.sprites = chunk.GetNativeArray(this.spriteType);
            this.transforms = chunk.GetComponentObjects(this.transformType, this.EntityManager);
            this.entities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                if (!ProcessThenReturnIfSuccess(i)) {
                    return false;
                }
            }

            return true;
        }

        private bool ProcessThenReturnIfSuccess(int index) {
            AddToSpriteLayer addToLayer = this.addToLayers[index];
            
            Maybe<SpriteLayer> layer = this.layers.Get(addToLayer.layerEntity);
            Assertion.Assert(layer.HasValue);
            SpriteLayer spriteLayer = layer.Value;

            // Resolve a SpriteManager with space
            Maybe<SpriteManager> manager = ResolveAvailable(ref spriteLayer);
            if (manager.HasValue) {
                // This means that the layer has an available manager
                // We directly add the sprite to the said manager
                AddSpriteToManager(manager.Value, index);

                return true;
            }

            // At this point, it means that the layer doesn't have an available sprite manager
            // We create a new one and skip the current frame
            Entity entity = this.PostUpdateCommands.CreateEntity();

            // Prepare a SpriteManager
            SpriteManager spriteManager = new SpriteManager(spriteLayer.allocationCount, this.PostUpdateCommands);
            spriteManager.SpriteLayerEntity = layer.Value.owner;
            spriteManager.SetMaterial(spriteLayer.material);
            spriteManager.Layer = spriteLayer.layer;
            spriteManager.SortingLayerId = spriteLayer.SortingLayerId;
            spriteManager.SortingLayer = spriteLayer.SortingLayer;
            spriteManager.AlwaysUpdateMesh = spriteLayer.alwaysUpdateMesh;
            this.PostUpdateCommands.AddSharedComponent(entity, spriteManager);

            return false;
        }

        private void AddSpriteToManager(SpriteManager manager, int index) {
            Sprite sprite = this.sprites[index];

            Assertion.Assert(manager.Owner != Entity.Null); // Should have been set already
            sprite.spriteManagerEntity = manager.Owner;

            Transform transform = this.transforms[index];
            float4x4 matrix = new float4x4(transform.rotation, transform.position);
            manager.Add(ref sprite, matrix);
            this.sprites[index] = sprite; // Modify the data

            // We add this component so it will be skipped by this system on the next frame
            this.PostUpdateCommands.AddComponent(this.entities[index],
                new Added(manager.Owner, sprite.managerIndex));
            
            // We add the shared component so that it can be filtered using such shared component
            // in other systems. For example, in SortRenderOrderSystem.
            this.PostUpdateCommands.AddSharedComponent(this.entities[index], manager);
        }

        private Maybe<SpriteManager> ResolveAvailable(ref SpriteLayer layer) {
            for (int i = 0; i < layer.spriteManagerEntities.Count; ++i) {
                Entity managerEntity = layer.spriteManagerEntities[i];
                Maybe<SpriteManager> result = this.managers.Get(managerEntity);
                if (result.HasValue && result.Value.HasAvailableSpace) {
                    return new Maybe<SpriteManager>(result.Value);
                }
            }

            return Maybe<SpriteManager>.Nothing;
        }
    }
}