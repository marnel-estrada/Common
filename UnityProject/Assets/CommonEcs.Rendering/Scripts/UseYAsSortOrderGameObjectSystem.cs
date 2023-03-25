using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UseYAsSortOrderGameObjectSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(
                typeof(Sprite), 
                typeof(Transform),
                ComponentType.ReadOnly<UseYAsSortOrder>(),
                ComponentType.Exclude<Static>()
            );
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            TransformAccessArray transforms = this.query.GetTransformAccessArray();
            NativeArray<TransformStash> stashes = new(transforms.length, Allocator.TempJob);
            
            // Job for copying to stashes
            StashTransformsJob stashTransforms = new() {
                stashes = stashes
            };
            JobHandle jobHandle = stashTransforms.Schedule(transforms, inputDeps);

            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(Allocator.TempJob);
            
            // Job for applying to sprites
            UpdateSpritesJob updateSpritesJob = new() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                useYAsSortOrderType = GetComponentTypeHandle<UseYAsSortOrder>(),
                stashes = stashes,
                chunkBaseEntityIndices = chunkBaseEntityIndices
            };

            jobHandle = updateSpritesJob.ScheduleParallel(this.query, jobHandle);
            
            // Don't forget to dispose
            jobHandle = chunkBaseEntityIndices.Dispose(jobHandle);
            jobHandle = stashes.Dispose(jobHandle);

            return jobHandle;
        }
        
        [BurstCompile]
        private struct UpdateSpritesJob : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<UseYAsSortOrder> useYAsSortOrderType;
            
            [ReadOnly]
            public NativeArray<TransformStash> stashes;

            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<UseYAsSortOrder> sortOrders = chunk.GetNativeArray(ref this.useYAsSortOrderType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                EntityIndexAide indexAide = new(ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntityIndex(out int i)) {
                    Sprite sprite = sprites[i];
                    UseYAsSortOrder sortOrder = sortOrders[i];

                    int index = indexAide.NextEntityIndexInQuery();
                    float3 position = this.stashes[index].position;
                
                    // We use negative of z here because the higher z should be ordered first
                    sprite.RenderOrder = -(position.y + sortOrder.offset);
                    
                    // Modify Sprite
                    sprites[i] = sprite;
                }
            }
        }
    }
}
