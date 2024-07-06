using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Filters sprites that changed and updates its values in the sprite manager.
    /// </summary>
    public partial class UpdateChangedComputeBufferSpritesSystem : SystemBase {
        private EntityQuery spritesQuery;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ComputeBufferSprite.Changed>()
                .WithAll<LocalTransform>()
                .WithAll<LocalToWorld>()
                .WithAll<UvIndex>()
                .WithAll<ManagerAdded>()
                .Build(this);
            RequireForUpdate(this.spritesQuery);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];

            UpdateSpritesJob updateSpritesJob = new() {
                spriteType = GetComponentTypeHandle<ComputeBufferSprite>(),
                localTransformType = GetComponentTypeHandle<LocalTransform>(),
                worldTransformType = GetComponentTypeHandle<LocalToWorld>(),
                changedType = GetComponentTypeHandle<ComputeBufferSprite.Changed>(),
                translationAndRotations = spriteManager.TranslationAndRotations,
                scales = spriteManager.Scales,
                colors = spriteManager.Colors
            };
            this.Dependency = updateSpritesJob.ScheduleParallel(this.spritesQuery, this.Dependency);
        }
        
        [BurstCompile]
        private struct UpdateSpritesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<LocalTransform> localTransformType;
            
            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> worldTransformType;

            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> translationAndRotations;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float> scales;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<Color> colors;

            private const int SPRITE_COUNT_PER_LAYER = 20000;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
                NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    if (sprite.managerIndex.IsNone) {
                        // Not added to the sprite manager yet
                        continue;
                    }
                    
                    LocalToWorld worldTransform = worldTransforms[i];
                    
                    int spriteManagerIndex = sprite.managerIndex.ValueOrError();
                    
                    // Position and rotation
                    float3 position = worldTransform.Position;
                    position.z = position.y + SPRITE_COUNT_PER_LAYER * sprite.layerOrder;
                    float rotation = worldTransform.Rotation.ToEuler().z;
                    this.translationAndRotations[spriteManagerIndex] = new float4(position, rotation);
                    
                    // Scale
                    LocalTransform localTransform = localTransforms[i];
                    this.scales[spriteManagerIndex] = localTransform.Scale;
                    
                    // Color
                    this.colors[spriteManagerIndex] = sprite.color;
                    
                    // Disable Changed so it will no longer be processed by this system
                    chunk.SetComponentEnabled(ref this.changedType, i, false);
                }
            }
        }
    }
}