using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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

                NativeArray<int> sortedIndices = spriteManager.SortedIndices;
                ResetSortedIndicesJob resetJob = new() {
                    sortedIndices = sortedIndices
                };
                handle = resetJob.ScheduleParallel(spriteCount, 64, handle);

                NativeParallelHashSet<int> transparentIndices = spriteManager.TransparentIndices;
                int transparentIndicesCount = transparentIndices.Count();
                if (transparentIndicesCount >= sortedIndices.Length) {
                    // This means that all sprites are transparent. No need to move them to the end. 
                    return;
                }

                MoveTransparentIndicesToTheEndJob moveTransparentToTheEndJob = new() {
                    transparentIndices = transparentIndices,
                    colors = spriteManager.Colors,
                    sortedIndices = sortedIndices
                };
                handle = moveTransparentToTheEndJob.Schedule(handle);
                
                // Sort the transparent entries by position and layer order
                // Note here that we only sort starting from the transparent entries
                SpriteIndexComparer comparer = new() {
                    translationsAndScales = spriteManager.TranslationsAndScales,
                    layerOrders = spriteManager.LayerOrderArray
                };
                
                int startIndex = sortedIndices.Length - transparentIndicesCount;
                handle = MultithreadedSort.SortWithComparer(ref sortedIndices, ref comparer, startIndex,
                    sortedIndices.Length - 1, handle);
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

                        if (transparentIndex >= lastCheckedIndex) {
                            // This means that the transparent index is already at the correct position
                            --lastCheckedIndex;
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

                        --lastCheckedIndex;
                        
                        swapped = true;
                    } while (!swapped);
                }
            }
        }

        private struct SpriteIndexComparer : IComparer<int> {
            [ReadOnly]
            public NativeArray<float4> translationsAndScales;

            [ReadOnly]
            public NativeArray<int> layerOrders;
            
            public int Compare(int a, int b) {
                // We multiply by negative value here because higher order means to be rendered later
                // This is also the calculation used in SpriteRendererIndexUv.shader
                // We negate Y because objects with lower Y should be rendered later. The lower it is, the higher
                // should the sprite be in the ordering.
                float4 aPos = this.translationsAndScales[a];
                float aOrder = -aPos.y + (this.layerOrders[a] * 0.001f);

                float4 bPos = this.translationsAndScales[b];
                float bOrder = -bPos.y + (this.layerOrders[b] * 0.001f);

                return aOrder.CompareTo(bOrder);
            }
        }
    }
}