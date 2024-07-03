using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
        }
        
        [BurstCompile]
        private struct UpdateSpritesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;

            [ReadOnly]
            public BufferTypeHandle<UvIndex> uvIndexType;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> translationAndRotations;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float> scales;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<Color> colors;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                BufferAccessor<UvIndex> uvIndexBuffers = chunk.GetBufferAccessor(ref this.uvIndexType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    // TODO Continue here
                }
            }
        }
    }
}