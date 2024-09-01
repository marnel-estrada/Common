using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// The job that identifies changes in the UV indices. Only checks for 1 uv index for now.
    /// </summary>
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    [UpdateBefore(typeof(UpdateChangedComputeBufferSpritesSystem))]
    [UpdateBefore(typeof(UpdateComputeBufferSpriteUvIndicesSystem))]
    public partial class IdentifyComputeBufferSpriteUvIndexChangedSystem : SystemBase {
        private EntityQuery spritesQuery;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<UvIndex>()
                .WithAll<ManagerAdded>()
                .WithNone<ComputeBufferSprite.Changed>()
                .Build(this);
            RequireForUpdate(this.spritesQuery);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            if (spriteManagers.Count <= 1) {
                // No SpriteManagers where created yet
                return;
            }
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];

            TrackUvIndexChangesJob trackUvIndexChangesJob = new() {
                spriteType = GetComponentTypeHandle<ComputeBufferSprite>(),
                uvIndexType = GetBufferTypeHandle<UvIndex>(),
                changedType = GetComponentTypeHandle<ComputeBufferSprite.Changed>(),
                uvBufferIndices = spriteManager.GetUvBufferIndices(0),
                lastSystemVersion = this.LastSystemVersion
            };
            this.Dependency = trackUvIndexChangesJob.ScheduleParallel(this.spritesQuery, this.Dependency);
        }
        
        [BurstCompile]
        private struct TrackUvIndexChangesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public BufferTypeHandle<UvIndex> uvIndexType;
            
            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
            
            [ReadOnly]
            public NativeArray<int> uvBufferIndices;
            
            public uint lastSystemVersion;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                if (!chunk.DidChange(ref this.uvIndexType, this.lastSystemVersion)) {
                    // No changes on the chunk. We can skip.
                    return;
                }
                
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                BufferAccessor<UvIndex> uvIndexBuffers = chunk.GetBufferAccessor(ref this.uvIndexType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ComputeBufferSprite sprite = sprites[i];
                    DynamicBuffer<UvIndex> uvIndices = uvIndexBuffers[i];

                    if (this.uvBufferIndices[sprite.managerIndex.ValueOrError()] == uvIndices[0].value) {
                        // No changes
                        continue;
                    }
                    
                    // UV index changed
                    chunk.SetComponentEnabled(ref this.changedType, i, true);
                }
            }
        }
    }
}