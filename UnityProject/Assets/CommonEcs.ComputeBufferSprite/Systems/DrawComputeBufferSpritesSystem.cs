using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DrawComputeBufferSpritesSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSpriteManager>()
                .Build(this);
        }
        
        private static readonly Bounds BOUNDS = new(Vector2.zero, Vector3.one * 100);

        protected override void OnUpdate() {
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(WorldUpdateAllocator);
            SharedComponentTypeHandle<ComputeBufferSpriteManager> spriteManagerType = 
                GetSharedComponentTypeHandle<ComputeBufferSpriteManager>();
            
            for (int i = 0; i < chunks.Length; i++) {
                ComputeBufferSpriteManager spriteManager = chunks[i].GetSharedComponentManaged(spriteManagerType, this.EntityManager);
                SortIndices(ref spriteManager);
                spriteManager.Draw(BOUNDS);
            }

            chunks.Dispose();
        }

        // Sorts the indices based on alpha
        private void SortIndices(ref ComputeBufferSpriteManager spriteManager) {
            JobHandle handle = new();

            try {
                int spriteCount = spriteManager.Count;
                
                ResetSortedIndicesJob resetJob = new() {
                    sortedIndices = spriteManager.SortedIndices
                };
                handle = resetJob.ScheduleParallel(spriteCount, 64, handle);
                
                if (spriteManager.TransparentIndices.Count() >= spriteManager.SortedIndices.Length) {
                    // This means that all sprites are transparent. No need to move them to the end. 
                    return;
                }

                MoveTransparentIndicesToTheEndJob moveTransparentToTheEndJob = new() {
                    transparentIndices = spriteManager.TransparentIndices,
                    colors = spriteManager.Colors,
                    sortedIndices = spriteManager.SortedIndices
                };
                handle = moveTransparentToTheEndJob.Schedule(handle);
            } finally {
                handle.Complete();
            }
        }
        
        [BurstCompile]
        private struct ResetSortedIndicesJob : IJobFor {
            [NativeDisableParallelForRestriction]
            public NativeArray<int> sortedIndices;
            
            public void Execute(int index) {
                this.sortedIndices[index] = index;
            }
        }

        [BurstCompile]
        private struct MoveTransparentIndicesToTheEndJob : IJob {
            [ReadOnly]
            public NativeParallelHashSet<int> transparentIndices;

            [ReadOnly]
            public NativeArray<Color> colors;

            public NativeArray<int> sortedIndices;
            
            public void Execute() {
                int lastCheckedIndex = this.sortedIndices.Length - 1; // Start at the end

                foreach (int transparentIndex in this.transparentIndices) {
                    // Look for an index to swap
                    bool swapped = false;
                    do {
                        if (lastCheckedIndex < 0) {
                            // Reached the end. No more index to swap
                            break;
                        }

                        if (transparentIndex > lastCheckedIndex) {
                            // This means that the transparent index is already at the correct position
                            break;
                        }
                        
                        if (this.colors[lastCheckedIndex].a < 0.99f ||
                            this.transparentIndices.Contains(lastCheckedIndex)) {
                            // Entry is transparent. Can't use this to swap.
                            --lastCheckedIndex;
                            continue;
                        }
                        
                        // At this point, we found a non-transparent entry
                        // We swap
                        sortedIndices[transparentIndex] = lastCheckedIndex;
                        sortedIndices[lastCheckedIndex] = transparentIndex;
                        swapped = true;
                    } while (!swapped);
                }
            }
        }
    }
}