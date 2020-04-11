using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderComputeBufferSpritesSystem))]
    public class UpdateDrawInstanceArraysSystem : SystemBase {
        private EntityQuery query;
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        private ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<ComputeBufferSprite>(), ComponentType.ReadOnly<ComputeBufferDrawInstance>());
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
            
            // We did it this way so it looks like the old way
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            this.managerQuery.Update();

            JobHandle lastHandle = inputDeps;
            
            this.spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            
            // Reset sort entries
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                lastHandle = new ResetSortEntriesJob() {
                    spriteMasterList = drawInstance.SpritesMasterList,
                    sortEntries = drawInstance.SortedEntries
                }.Schedule(drawInstance.SpriteCount, 64, lastHandle);
            }
            
            // Sort the entries
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                lastHandle = new SortJob() {
                    sortEntries = drawInstance.SortedEntries
                }.Schedule(lastHandle);
            }

            // Note here that we start iteration from 1 because the first value is the default value
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                lastHandle = UpdateArrays(lastHandle, drawInstance);
            }

            return lastHandle;
        }

        private JobHandle UpdateArrays(JobHandle inputDeps, ComputeBufferDrawInstance drawInstance) {
            this.query.SetSharedComponentFilter(drawInstance);
            
            JobHandle handle = inputDeps;
            drawInstance.ExpandArrays(drawInstance.SpriteCount);

            handle = new SetValuesJob() {
                spriteMasterList = drawInstance.SpritesMasterList,
                sortEntries = drawInstance.SortedEntries,
                matrices = drawInstance.Matrices,
                sizePivots = drawInstance.SizePivots,
                uvs = drawInstance.Uvs,
                colors = drawInstance.Colors
            }.Schedule(drawInstance.SpriteCount, 64, handle);

            return handle;
        }

        [BurstCompile]
        private struct ResetSortEntriesJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> spriteMasterList;

            [NativeDisableParallelForRestriction]
            public NativeArray<SortedEntry> sortEntries;

            public void Execute(int index) {
                ComputeBufferSprite sprite = this.spriteMasterList[index];
                this.sortEntries[index] = new SortedEntry(index, sprite.layerOrder, sprite.renderOrder); 
            }
        }
        
        [BurstCompile]
        private struct SortJob : IJob {
            public NativeArray<SortedEntry> sortEntries;

            public void Execute() {
                if (this.sortEntries.Length > 0) {
                    Quicksort(0, this.sortEntries.Length - 1);
                }
            }

            private void Quicksort(int left, int right) {
                int i = left;
                int j = right;
                SortedEntry pivot = this.sortEntries[(left + right) / 2];

                while (i <= j) {
                    // Lesser
                    while (Compare(this.sortEntries[i], ref pivot) < 0) {
                        ++i;
                    }

                    // Greater
                    while (Compare(this.sortEntries[j], ref pivot) > 0) {
                        --j;
                    }

                    if (i <= j) {
                        // Swap
                        SortedEntry temp = this.sortEntries[i];
                        this.sortEntries[i] = this.sortEntries[j];
                        this.sortEntries[j] = temp;

                        ++i;
                        --j;
                    }
                }

                // Recurse
                if (left < j) {
                    Quicksort(left, j);
                }

                if (i < right) {
                    Quicksort(i, right);
                }
            }

            private int Compare(SortedEntry a, ref SortedEntry b) {
                if (a.layerOrder < b.layerOrder) {
                    return -1;
                }

                if (a.layerOrder > b.layerOrder) {
                    return 1;
                }

                // At this point, they have the same layerOrder
                // We check the renderOrder
                if (a.renderOrder < b.renderOrder) {
                    return -1;
                }

                if (a.renderOrder > b.renderOrder) {
                    return 1;
                }

                // They are equal
                return 0;
            }
        }
        
        [BurstCompile]
        private struct SetValuesJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> spriteMasterList;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<SortedEntry> sortEntries;
            
            public NativeArray<float4x4> matrices;
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;
            
            public void Execute(int index) {
                int spriteIndex = this.sortEntries[index].index;
                ComputeBufferSprite sprite = this.spriteMasterList[spriteIndex];
                
                this.matrices[index] = sprite.localToWorld;
                this.sizePivots[index] = new float4(sprite.size, sprite.pivot);
                this.uvs[index] = sprite.Uv;
                this.colors[index] = sprite.Color;
            }
        }
    }
}