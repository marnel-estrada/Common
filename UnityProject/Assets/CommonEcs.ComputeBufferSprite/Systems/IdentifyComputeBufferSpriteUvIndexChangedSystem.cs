﻿using System.Collections.Generic;
using Common;
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
        
        private SharedComponentQuery<ComputeBufferSpriteManager>? spriteManagerQuery;

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
            if (this.spriteManagerQuery == null) {
                throw new CantBeNullException(nameof(this.spriteManagerQuery));
            }
            
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
                managerAddedType = GetComponentTypeHandle<ManagerAdded>(),
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
            public ComponentTypeHandle<ManagerAdded> managerAddedType;

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
                
                NativeArray<ManagerAdded> managerAddedComponents = chunk.GetNativeArray(ref this.managerAddedType);
                BufferAccessor<UvIndex> uvIndexBuffers = chunk.GetBufferAccessor(ref this.uvIndexType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ManagerAdded managerAdded = managerAddedComponents[i];
                    DynamicBuffer<UvIndex> uvIndices = uvIndexBuffers[i];
                    if (uvIndices.Length == 0) {
                        // No UV indices specified yet
                        continue;
                    }

                    if (this.uvBufferIndices[managerAdded.managerIndex] == uvIndices[0].value) {
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