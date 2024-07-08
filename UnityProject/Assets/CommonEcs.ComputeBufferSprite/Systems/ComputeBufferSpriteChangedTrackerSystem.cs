using System.Collections.Generic;
using Common;
using CommonEcs.Scripts.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Determines if a sprite has different values from the values in the main array and
    /// sets ComputeBufferSprite.Changed to enabled so that UpdateChangedComputeBufferSpritesSystem
    /// can pick it up. We did it this way so that systems that alter transform, for example, don't need
    /// to know about setting ComputeBufferSprite.Changed to enabled.  
    /// </summary>
    public partial class ComputeBufferSpriteChangedTrackerSystem : SystemBase {
        private EntityQuery spritesQuery;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            // Note here that we only consider sprites where Changed was not enabled yet
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<LocalTransform>()
                .WithAll<LocalToWorld>()
                .WithAll<ManagerAdded>()
                .WithNone<ComputeBufferSprite.Changed>()
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
            
            // TODO Schedule job
            TrackChangedJob trackChangedJob = new() {
                lastSystemVersion = this.LastSystemVersion
            };
        }
        
        [BurstCompile]
        private struct TrackChangedJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<LocalTransform> localTransformType;

            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> worldTransformType;

            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
            
            [ReadOnly]
            public NativeArray<float4> translationAndRotations;
            
            [ReadOnly]
            public NativeArray<float> scales;
            
            [ReadOnly]
            public NativeArray<Color> colors;
            
            public uint lastSystemVersion;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                // Check if there were changes before continuing
                if (!(chunk.DidChange(ref this.spriteType, this.lastSystemVersion) 
                      || chunk.DidChange(ref this.localTransformType, this.lastSystemVersion)
                      || chunk.DidChange(ref this.worldTransformType, this.lastSystemVersion))) {
                    return;
                }

                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
                NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    LocalTransform localTransform = localTransforms[i];
                    LocalToWorld worldTransform = worldTransforms[i];

                    int managerIndex = sprite.managerIndex.ValueOrError();
                    
                    // Check position and rotation
                    float4 positionAndRot = this.translationAndRotations[managerIndex];
                    float3 position = worldTransform.Position;
                    if (!position.TolerantEquals(positionAndRot.xyz)) {
                        // Changed position
                        chunk.SetComponentEnabled(ref this.changedType, i, true);
                        continue;
                    }
                    
                    float rotation = worldTransform.Rotation.ToEuler().z;
                    if (!rotation.TolerantEquals(positionAndRot.w)) {
                        // Changed rotation
                        chunk.SetComponentEnabled(ref this.changedType, i, true);
                        continue;
                    }
                    
                    // Check scale
                    float scaleInManager = this.scales[managerIndex];
                    float scale = localTransform.Scale;
                    if (!scale.TolerantEquals(scaleInManager)) {
                        // Changed scale
                        chunk.SetComponentEnabled(ref this.changedType, i, true);
                        continue;
                    }
                    
                    // Check color
                    Color colorInManager = this.colors[managerIndex];
                    if (!sprite.color.TolerantEquals(colorInManager)) {
                        // Changed scale
                        chunk.SetComponentEnabled(ref this.changedType, i, true);
                    }
                }
            }
        }
    }
}