﻿using System.Collections.Generic;
using CommonEcs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems {
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    public partial class AddComputeBufferSpriteToManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;
        private EntityQuery spritesQuery;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithNone<ManagerAdded>()
                .Build(this);
        }

        private EntityTypeHandle entityType;
        private ComponentTypeHandle<ComputeBufferSprite> spriteType;
        private ComponentTypeHandle<LocalTransform> localTransformType;
        private ComponentTypeHandle<LocalToWorld> worldTransformType;

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];

            this.entityType = GetEntityTypeHandle();
            this.spriteType = GetComponentTypeHandle<ComputeBufferSprite>();
            this.localTransformType = GetComponentTypeHandle<LocalTransform>(true);
            this.worldTransformType = GetComponentTypeHandle<LocalToWorld>(true);

            // We can't use Burst compiled jobs here since the sprite manager have managed types
            // like ComputeBuffer
            NativeArray<ArchetypeChunk> chunks = this.spritesQuery.ToArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();

            for (int i = 0; i < chunks.Length; i++) {
                ProcessChunk(chunks[i], ref spriteManager, ref commandBuffer);
            }
            
            chunks.Dispose();
        }

        private void ProcessChunk(ArchetypeChunk chunk, ref ComputeBufferSpriteManager spriteManager,
            ref EntityCommandBuffer commandBuffer) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
            NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
            NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);

            for (int i = 0; i < chunk.Count; i++) {
                ComputeBufferSprite sprite = sprites[i];
                LocalTransform localTransform = localTransforms[i];
                LocalToWorld worldTransform = worldTransforms[i];

                float rotation = math.Euler(worldTransform.Rotation).z;
                spriteManager.Add(ref sprite, worldTransform.Position, rotation, localTransform.Scale);
                sprites[i] = sprite; // We modify since the managerIndex would be assigned on add
                
                // Add this component so it will no longer be processed by this system
                commandBuffer.AddComponent(entities[i], new ManagerAdded(sprite.managerIndex));
            }
        }
    }
}