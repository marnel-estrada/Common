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
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = 
                new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
        }
        
        private static readonly Bounds BOUNDS = new(Vector2.zero, Vector3.one * 100);

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];
            SortIndices(ref spriteManager);
            spriteManager.Draw(BOUNDS);
        }

        // Sorts the indices based on alpha
        private void SortIndices(ref ComputeBufferSpriteManager spriteManager) {
            JobHandle handle = new();

            int spriteCount = spriteManager.Count;

            NativeArray<int> sortedIndices = spriteManager.SortedIndices;
            ResetSortedIndicesJob resetJob = new() {
                sortedIndices = sortedIndices
            };
            handle = resetJob.ScheduleParallel(spriteCount, 64, handle);

            NativeParallelHashSet<int> transparentIndices = new(spriteCount, WorldUpdateAllocator);
            CollectTransparentIndicesJob collectTransparentIndicesJob = new() {
                colors = spriteManager.Colors,
                resultSet = transparentIndices.AsParallelWriter()
            };
            handle = collectTransparentIndicesJob.ScheduleParallel(spriteCount, 64, handle);

            MoveTransparentIndicesToTheEndJob moveTransparentToTheEndJob = new() {
                transparentIndices = transparentIndices,
                colors = spriteManager.Colors,
                sortedIndices = sortedIndices,
                spriteCount = spriteCount
            };
            handle = moveTransparentToTheEndJob.Schedule(handle);
            
            handle.Complete();
                
            // // Sort the transparent entries by position and layer order
            // // Note here that we only sort starting from the transparent entries
            // SpriteIndexComparer comparer = new() {
            //     translationsAndScales = spriteManager.TranslationsAndScales,
            //     layerOrders = spriteManager.LayerOrderArray
            // };
            //
            // int startIndex = spriteCount - transparentIndices.Count();
            // handle = MultithreadedSort.SortWithComparer(ref sortedIndices, ref comparer, startIndex,
            //     spriteCount - 1, handle);
            // handle.Complete();
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
        private struct CollectTransparentIndicesJob : IJobFor {
            [ReadOnly]
            public NativeArray<Color> colors;
            
            public NativeParallelHashSet<int>.ParallelWriter resultSet;
            
            public void Execute(int index) {
                if (colors[index].a < 0.99f) {
                    this.resultSet.Add(index);
                }
            }
        }

        [BurstCompile]
        private struct MoveTransparentIndicesToTheEndJob : IJob {
            [ReadOnly]
            public NativeParallelHashSet<int> transparentIndices;

            [ReadOnly]
            public NativeArray<Color> colors;

            public NativeArray<int> sortedIndices;

            public int spriteCount;
            
            public void Execute() {
                int lastCheckedIndex = spriteCount - 1; // Start at the end
                int firstTransparentIndex = spriteCount - this.transparentIndices.Count();

                foreach (int transparentIndex in this.transparentIndices) {
                    // Look for an index to swap
                    bool swapped = false;
                    do {
                        if (transparentIndex >= firstTransparentIndex) {
                            // This means that the index is already part of the transparent set at the end
                            // No need to move it.
                            break;
                        }
                        
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