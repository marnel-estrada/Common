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
                .WithPresent<Active>()
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
                managerAddedType = GetComponentTypeHandle<ManagerAdded>(),
                layerType = GetSharedComponentTypeHandle<ComputeBufferSpriteLayer>(),
                localTransformType = GetComponentTypeHandle<LocalTransform>(),
                worldTransformType = GetComponentTypeHandle<LocalToWorld>(),
                activeType = GetComponentTypeHandle<Active>(),
                translationsAndScales = spriteManager.TranslationsAndScales,
                rotations = spriteManager.Rotations,
                sizes = spriteManager.Sizes,
                pivots = spriteManager.Pivots,
                colors = spriteManager.Colors,
                activeArray = spriteManager.ActiveArray
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
            public ComponentTypeHandle<ManagerAdded> managerAddedType;

            [ReadOnly]
            public SharedComponentTypeHandle<ComputeBufferSpriteLayer> layerType;

            [ReadOnly]
            public ComponentTypeHandle<LocalTransform> localTransformType;
            
            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> worldTransformType;

            [ReadOnly]
            public ComponentTypeHandle<Active> activeType;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> translationsAndScales;

            [NativeDisableParallelForRestriction]
            public NativeArray<float4> rotations;

            [NativeDisableParallelForRestriction]
            public NativeArray<float2> sizes;

            [NativeDisableParallelForRestriction]
            public NativeArray<float2> pivots;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<Color> colors;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<int> activeArray;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<ManagerAdded> managerAddedComponents = chunk.GetNativeArray(ref this.managerAddedType);
                NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
                NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);
                ComputeBufferSpriteLayer layer = chunk.GetSharedComponent(this.layerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    LocalToWorld worldTransform = worldTransforms[i];
                    
                    int spriteManagerIndex = managerAddedComponents[i].managerIndex;
                    
                    // Position
                    // We negate the layer value since sprites at a higher layer should be at the front more
                    float3 position = worldTransform.Position;
                    position.z += ComputeBufferSpriteUtils.ComputeZPos(layer.value, position.y);
                    LocalTransform localTransform = localTransforms[i];
                    this.translationsAndScales[spriteManagerIndex] = new float4(position, localTransform.Scale);
                    
                    // Rotation
                    this.rotations[spriteManagerIndex] = worldTransform.Rotation.value;
                    
                    // Size
                    this.sizes[spriteManagerIndex] = sprite.size;
                    
                    // Pivot
                    this.pivots[spriteManagerIndex] = sprite.pivot;
                    
                    // Color
                    this.colors[spriteManagerIndex] = sprite.color;
                    
                    // Active
                    bool isActive = chunk.IsComponentEnabled(ref this.activeType, i);
                    this.activeArray[spriteManagerIndex] = isActive ? 1 : 0;
                }
            }
        }
    }
}