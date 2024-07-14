using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Updates the UV indices for changed sprites. Also handles updating of multiple UVs if there
    /// are multiple specified in the sprite manager
    /// </summary>
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    [UpdateBefore(typeof(ResetComputeBufferSpriteChangedSystem))]
    public partial class UpdateComputeBufferSpriteUvIndicesSystem : SystemBase {
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

            switch (spriteManager.UvIndicesBufferCount) {
                case 1: {
                    // Schedule single UV buffer
                    UpdateSingleUvIndicesJob updateUvIndicesJob = new() {
                        spriteType = GetComponentTypeHandle<ComputeBufferSprite>(),
                        uvIndexType = GetBufferTypeHandle<UvIndex>(),
                        uvBufferIndices = spriteManager.GetUvBufferIndices(0)
                    };
                    this.Dependency = updateUvIndicesJob.ScheduleParallel(this.spritesQuery, this.Dependency);
                    break;
                }
                
                case 2: {
                    // Schedule double UV buffer
                    UpdateDoubleUvIndicesJob updateUvIndicesJob = new() {
                        spriteType = GetComponentTypeHandle<ComputeBufferSprite>(),
                        uvIndexType = GetBufferTypeHandle<UvIndex>(),
                        firstUvBufferIndices = spriteManager.GetUvBufferIndices(0),
                        secondUvBufferIndices = spriteManager.GetUvBufferIndices(1)
                    };
                    this.Dependency = updateUvIndicesJob.ScheduleParallel(this.spritesQuery, this.Dependency);
                    break;
                }
            }
        }
        
        // Updates only one uv indices
        [BurstCompile]
        private struct UpdateSingleUvIndicesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public BufferTypeHandle<UvIndex> uvIndexType;

            [NativeDisableParallelForRestriction]
            public NativeArray<int> uvBufferIndices;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                BufferAccessor<UvIndex> uvIndicesBuffers = chunk.GetBufferAccessor(ref this.uvIndexType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    DynamicBuffer<UvIndex> uvIndices = uvIndicesBuffers[i];
                    
                    // Set only the first uv index
                    this.uvBufferIndices[sprite.managerIndex.ValueOrError()] = uvIndices[0].value;
                }
            }
        }
        
        // Updates two uv indices
        [BurstCompile]
        private struct UpdateDoubleUvIndicesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public BufferTypeHandle<UvIndex> uvIndexType;

            [NativeDisableParallelForRestriction]
            public NativeArray<int> firstUvBufferIndices;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<int> secondUvBufferIndices;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                BufferAccessor<UvIndex> uvIndicesBuffers = chunk.GetBufferAccessor(ref this.uvIndexType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    DynamicBuffer<UvIndex> uvIndices = uvIndicesBuffers[i];

                    int managerIndex = sprite.managerIndex.ValueOrError();
                    this.firstUvBufferIndices[managerIndex] = uvIndices[0].value;
                    this.secondUvBufferIndices[managerIndex] = uvIndices[1].value;
                }
            }
        }
    }
}
