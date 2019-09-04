using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using Common;

namespace CommonEcs {
    /// <summary>
    /// This is the same as AddGameObjectSpriteToLayerSystem but for sprites in pure ECS
    /// </summary>
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(SpriteLayerInstancesSystem))]
    [UpdateBefore(typeof(AddSpriteToManagerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddSpritesToLayerSystem : ComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkComponentType<AddToSpriteLayer> addToLayerType;
        private ArchetypeChunkComponentType<Sprite> spriteType;
        private ArchetypeChunkComponentType<LocalToWorld> matrixType;
        private ArchetypeChunkEntityType entityType;

        private SpriteLayerInstancesSystem layers;
        private SpriteManagerInstancesSystem managers;

        protected override void OnCreate() {
            this.layers = this.World.GetOrCreateSystem<SpriteLayerInstancesSystem>();
            this.managers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
            
            this.query = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(AddToSpriteLayer), typeof(Sprite), typeof(LocalToWorld)
            }, new ComponentType[] {
                typeof(Added)
            }));
        }

        protected override void OnUpdate() {
            this.addToLayerType = GetArchetypeChunkComponentType<AddToSpriteLayer>();
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.matrixType = GetArchetypeChunkComponentType<LocalToWorld>();
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
        private NativeArray<LocalToWorld> matrices;
        private NativeArray<Entity> entities;

        // Return true if processing is successful
        private bool Process(ArchetypeChunk chunk) {
            this.addToLayers = chunk.GetNativeArray(this.addToLayerType);
            this.sprites = chunk.GetNativeArray(this.spriteType);
            this.matrices = chunk.GetNativeArray(this.matrixType);
            this.entities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                AddToSpriteLayer addToLayer = this.addToLayers[i];
                Maybe<SpriteLayer> layer = this.layers.Get(addToLayer.layerEntity);
                Assertion.Assert(layer.HasValue);
                SpriteLayer spriteLayer = layer.Value;

                // Resolve a SpriteManager with space
                Maybe<SpriteManager> manager = ResolveAvailable(ref spriteLayer);
                if (manager.HasValue) {
                    // This means that the layer has an available manager
                    // We directly add the sprite to the said manager
                    AddSpriteToManager(manager.Value, i);
                } else {
                    // At this point, it means that the layer doesn't have an available sprite manager
                    // We create a new one and skip the current frame
                    CreateSpriteManagerEntity(spriteLayer, addToLayer.layerEntity);
                    return false;
                }
            }

            return true;
        }

        private void CreateSpriteManagerEntity(SpriteLayer spriteLayer, Entity spriteLayerEntity) {
            Entity entity = this.PostUpdateCommands.CreateEntity();

            // Prepare a SpriteManager
            SpriteManager spriteManager = new SpriteManager(spriteLayer.allocationCount, this.PostUpdateCommands);
            spriteManager.SpriteLayerEntity = spriteLayer.owner;
            spriteManager.SetMaterial(spriteLayer.material);
            spriteManager.Layer = spriteLayer.layer;
            spriteManager.SortingLayer = spriteLayer.SortingLayer;
            spriteManager.SortingLayerId = spriteLayer.SortingLayerId;
            spriteManager.AlwaysUpdateMesh = spriteLayer.alwaysUpdateMesh;
            spriteManager.UseMeshRenderer = spriteLayer.useMeshRenderer;
            this.PostUpdateCommands.AddSharedComponent(entity, spriteManager);

            if (spriteLayer.useMeshRenderer) {
                // This means that the layer will use MeshRenderers in GameObjects to render the mesh
                MeshRendererVessel vessel = new MeshRendererVessel(spriteLayerEntity, spriteLayer.material, spriteLayer.layer,
                    spriteLayer.SortingLayerId);
                this.PostUpdateCommands.AddSharedComponent(entity, vessel);
            }
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

        private void AddSpriteToManager(SpriteManager manager, int index) {
            Sprite sprite = this.sprites[index];

            Assertion.Assert(manager.Owner != Entity.Null); // Should have been set already
            sprite.spriteManagerEntity = manager.Owner;

            LocalToWorld matrix = this.matrices[index];
            manager.Add(ref sprite, matrix.Value);
            this.sprites[index] = sprite; // Modify the data

            // We add this component so it will be skipped by this system on the next frame
            this.PostUpdateCommands.AddComponent(this.entities[index], new Added(manager.Owner, sprite.managerIndex));

            // We add the shared component so that it can be filtered using such shared component
            // in other systems. For example, in SortRenderOrderSystem.
            this.PostUpdateCommands.AddSharedComponent(this.entities[index], manager);
        }

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
    }
}