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
    public partial class AddGameObjectSpriteToLayerSystem : SystemBase {
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

        private SpriteLayerInstancesSystem layersSystem;
        private SpriteManagerInstancesSystem managersSystem;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.layersSystem = this.World.GetOrCreateSystemManaged<SpriteLayerInstancesSystem>();
            this.managersSystem = this.World.GetOrCreateSystemManaged<SpriteManagerInstancesSystem>();
            this.commandBufferSystem = this.World.GetOrCreateSystemManaged<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            bool hasManager = true;
            
            this.Entities.WithNone<Added>().ForEach((Entity entity, Transform transform, ref Sprite sprite, ref AddToSpriteLayer addToLayer) => {
                if (!hasManager) {
                    // Do not process anymore if it already returned false in a previous entity
                    return;
                }

                hasManager = ProcessThenReturnIfSuccess(entity, transform, ref sprite, ref addToLayer, commandBuffer);
            }).WithoutBurst().Run();
        }

        private bool ProcessThenReturnIfSuccess(Entity spriteEntity, Transform transform, ref Sprite sprite, ref AddToSpriteLayer addToLayer, EntityCommandBuffer commandBuffer) {
            Maybe<SpriteLayer> layer = this.layersSystem.Get(addToLayer.layerEntity);
            Assertion.IsTrue(layer.HasValue);
            SpriteLayer spriteLayer = layer.Value;

            // Resolve a SpriteManager with space
            Maybe<SpriteManager> manager = ResolveAvailable(ref spriteLayer);
            if (manager.HasValue) {
                // This means that the layer has an available manager
                // We directly add the sprite to the said manager
                AddSpriteToManager(manager.Value, spriteEntity, transform, ref sprite, commandBuffer);

                return true;
            }

            // At this point, it means that the layer doesn't have an available sprite manager
            // We create a new one and skip the current frame
            Entity spriteManagerEntity = commandBuffer.CreateEntity();

            // Prepare a SpriteManager
            SpriteManager spriteManager = new(spriteLayer.allocationCount);
            spriteManager.SpriteLayerEntity = layer.Value.owner;
            spriteManager.SetMaterial(spriteLayer.material);
            spriteManager.Layer = spriteLayer.layer;
            spriteManager.SortingLayerId = spriteLayer.SortingLayerId;
            spriteManager.SortingLayer = spriteLayer.SortingLayer;
            spriteManager.AlwaysUpdateMesh = spriteLayer.alwaysUpdateMesh;
            commandBuffer.AddSharedComponentManaged(spriteManagerEntity, spriteManager);

            return false;
        }

        private static void AddSpriteToManager(SpriteManager manager, Entity spriteEntity, Transform transform, ref Sprite sprite, EntityCommandBuffer commandBuffer) {
            Assertion.IsTrue(manager.Owner != Entity.Null); // Should have been set already
            sprite.spriteManagerEntity = manager.Owner;

            float4x4 matrix = new float4x4(transform.rotation, transform.position);
            manager.Add(ref sprite, matrix);

            // We add this component so it will be skipped by this system on the next frame
            commandBuffer.AddComponent(spriteEntity,
                new Added(manager.Owner, sprite.managerIndex));
            
            // We add the shared component so that it can be filtered using such shared component
            // in other systems. For example, in SortRenderOrderSystem.
            commandBuffer.AddSharedComponentManaged(spriteEntity, manager);

            if (manager.AlwaysUpdateMesh) {
                // We add this component so it will be excluded in IdentifySpriteManagerChangedSystem
                commandBuffer.AddComponent(spriteEntity, new AlwaysUpdateMesh());
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
    }
}