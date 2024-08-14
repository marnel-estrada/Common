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
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    [UpdateBefore(typeof(ResetComputeBufferSpriteChangedSystem))]
    public partial class UpdateChangedComputeBufferSpritesSystem : SystemBase {
        private EntityQuery spritesQuery;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ComputeBufferSprite.Changed>()
                .WithAll<ComputeBufferSpriteLayer>()
                .WithAll<LocalTransform>()
                .WithAll<LocalToWorld>()
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
                layerType = GetSharedComponentTypeHandle<ComputeBufferSpriteLayer>(),
                localTransformType = GetComponentTypeHandle<LocalTransform>(),
                worldTransformType = GetComponentTypeHandle<LocalToWorld>(),
                translationsAndScales = spriteManager.TranslationsAndScales,
                rotations = spriteManager.Rotations,
                colors = spriteManager.Colors
            };
            this.Dependency = updateSpritesJob.ScheduleParallel(this.spritesQuery, this.Dependency);
        }
        
        /// <summary>
        /// This is faster than scheduling jobs for different changes
        /// </summary>
        [BurstCompile]
        private struct UpdateSpritesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public SharedComponentTypeHandle<ComputeBufferSpriteLayer> layerType;

            [ReadOnly]
            public ComponentTypeHandle<LocalTransform> localTransformType;
            
            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> worldTransformType;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> translationsAndScales;

            [NativeDisableParallelForRestriction]
            public NativeArray<float4> rotations;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<Color> colors;

            private const int SPRITE_COUNT_PER_LAYER = 20000;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
                NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);
                ComputeBufferSpriteLayer layer = chunk.GetSharedComponent(this.layerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    LocalToWorld worldTransform = worldTransforms[i];
                    
                    int spriteManagerIndex = sprite.managerIndex.ValueOrError();
                    
                    // Position
                    // We negate the layer value since sprites at a higher layer should be at the front more
                    float3 position = worldTransform.Position;
                    position.z = (-layer.value * 5) + (position.y / SPRITE_COUNT_PER_LAYER);
                    LocalTransform localTransform = localTransforms[i];
                    this.translationsAndScales[spriteManagerIndex] = new float4(position, localTransform.Scale);
                    
                    // Rotation
                    this.rotations[spriteManagerIndex] = worldTransform.Rotation.value;
                    
                    // Color
                    this.colors[spriteManagerIndex] = sprite.color;
                }
            }
        }
    }
}