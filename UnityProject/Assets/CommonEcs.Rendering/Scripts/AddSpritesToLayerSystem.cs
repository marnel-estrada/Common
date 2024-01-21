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
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class AddSpritesToLayerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private EntityQuery query;

        private ComponentTypeHandle<AddToSpriteLayer> addToLayerType;
        private ComponentTypeHandle<Sprite> spriteType;
        private ComponentTypeHandle<LocalToWorld> matrixType;
        private EntityTypeHandle entityType;

        private SpriteLayerInstancesSystem layersSystem;
        private SpriteManagerInstancesSystem managersSystem;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            this.layersSystem = this.GetOrCreateSystemManaged<SpriteLayerInstancesSystem>();
            this.managersSystem = this.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();
            
            this.query = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                ComponentType.ReadOnly<AddToSpriteLayer>(), typeof(Sprite), ComponentType.ReadOnly<LocalToWorld>(), 
            }, new ComponentType[] {
                typeof(Added)
            }));
        }

        protected override void OnUpdate() {
            // We have to complete here since this system does not use jobs due to usage of reference
            // types like SpriteManager (has Mesh and Material).
            // It will cause a "must call Complete()" error if there are other jobs referencing a component 
            // handle to the Sprite component.
            this.Dependency.Complete();
            
            this.addToLayerType = GetComponentTypeHandle<AddToSpriteLayer>();
            this.spriteType = GetComponentTypeHandle<Sprite>();
            this.matrixType = GetComponentTypeHandle<LocalToWorld>(true);
            this.entityType = GetEntityTypeHandle();

            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                if (!Process(chunks[i])) {
                    break;
                }
            }

            chunks.Dispose();
        }

        private NativeArray<Entity> entities;
        private NativeArray<AddToSpriteLayer> addToLayers;
        private NativeArray<Sprite> sprites;
        private NativeArray<LocalToWorld> matrices;

        // Return true if processing is successful
        private bool Process(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.addToLayers = chunk.GetNativeArray(ref this.addToLayerType);
            this.sprites = chunk.GetNativeArray(ref this.spriteType);
            this.matrices = chunk.GetNativeArray(ref this.matrixType);

            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();

            for (int i = 0; i < chunk.Count; ++i) {
                AddToSpriteLayer addToLayer = this.addToLayers[i];
                if (addToLayer.layerEntity == Entity.Null) {
                    // Might not be prepared yet
                    continue;
                }
                
                Maybe<SpriteLayer> layer = this.layersSystem.Get(addToLayer.layerEntity);
                Assertion.IsTrue(layer.HasValue);
                SpriteLayer spriteLayer = layer.Value;

                // Resolve a SpriteManager with space
                Maybe<SpriteManager> manager = ResolveAvailable(ref spriteLayer);
                if (manager.HasValue) {
                    // This means that the layer has an available manager
                    // We directly add the sprite to the said manager
                    AddSpriteToManager(manager.Value, i, ref commandBuffer);
                } else {
                    // At this point, it means that the layer doesn't have an available sprite manager
                    // We create a new one and skip the current frame
                    CreateSpriteManagerEntity(spriteLayer, addToLayer.layerEntity, ref commandBuffer);
                    return false;
                }
            }

            return true;
        }

        private void CreateSpriteManagerEntity(SpriteLayer spriteLayer, Entity spriteLayerEntity, 
            ref EntityCommandBuffer commandBuffer) {
            Entity entity = commandBuffer.CreateEntity();

            // Prepare a SpriteManager
            SpriteManager spriteManager = new(spriteLayer.allocationCount);
            spriteManager.Name = spriteLayer.Name; // Copy name for debugging purposes
            spriteManager.SpriteLayerEntity = spriteLayer.owner;
            spriteManager.SetMaterial(spriteLayer.material);
            spriteManager.Layer = spriteLayer.layer;
            spriteManager.SortingLayer = spriteLayer.SortingLayer;
            spriteManager.SortingLayerId = spriteLayer.SortingLayerId;
            spriteManager.AlwaysUpdateMesh = spriteLayer.alwaysUpdateMesh;
            spriteManager.UseMeshRenderer = spriteLayer.useMeshRenderer;
            commandBuffer.AddSharedComponentManaged(entity, spriteManager);

            if (spriteLayer.useMeshRenderer) {
                // This means that the layer will use MeshRenderers in GameObjects to render the mesh
                MeshRendererVessel vessel = new(spriteLayerEntity, spriteLayer.Name, spriteLayer.material, 
                    spriteLayer.layer, spriteLayer.SortingLayerId, spriteLayer.SortingOrder);
                commandBuffer.AddSharedComponentManaged(entity, vessel);
            }
        }

        private Maybe<SpriteManager> ResolveAvailable(ref SpriteLayer layer) {
            for (int i = 0; i < layer.spriteManagerEntities.Count; ++i) {
                Entity managerEntity = layer.spriteManagerEntities[i];
                Maybe<SpriteManager> result = this.managersSystem.Get(managerEntity);
                if (result.HasValue && result.Value.HasAvailableSpace) {
                    return new Maybe<SpriteManager>(result.Value);
                }
            }

            return Maybe<SpriteManager>.Nothing;
        }

        private void AddSpriteToManager(SpriteManager manager, int index, ref EntityCommandBuffer commandBuffer) {
            Sprite sprite = this.sprites[index];

            Assertion.IsTrue(manager.Owner != Entity.Null); // Should have been set already
            sprite.spriteManagerEntity = manager.Owner;

            LocalToWorld matrix = this.matrices[index];
            manager.Add(ref sprite, matrix.Value);
            this.sprites[index] = sprite; // Modify the data

            // We add this component so it will be skipped by this system on the next frame
            commandBuffer.AddComponent(this.entities[index], new Added(manager.Owner, sprite.managerIndex));

            // We add the shared component so that it can be filtered using such shared component
            // in other systems. For example, in SortRenderOrderSystem.
            commandBuffer.AddSharedComponentManaged(this.entities[index], manager);
        }

        public struct Added : ICleanupComponentData {
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