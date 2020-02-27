using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateBefore(typeof(UpdateChangedVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateBefore(typeof(SpriteManagerJobsFinisher))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SortRenderOrderSystem : JobComponentSystem {
        private EntityQuery query;

        private SharedComponentQuery<SpriteManager> managerQuery;

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<Sprite>(), ComponentType.ReadOnly<SpriteManager>());
            this.managerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.managerQuery.Update();
            
            // Clear first
            ClearSortLists();
            
            JobHandle lastHandle = inputDeps;

            IReadOnlyList<SpriteManager> managers = this.managerQuery.SharedComponents;
            ArchetypeChunkComponentType<Sprite> spriteType = GetArchetypeChunkComponentType<Sprite>(true);

            // Add jobs
            // Note here that we start iteration from 1 because the first SpriteManager is the default value
            for (int i = 1; i < managers.Count; ++i) {
                SpriteManager spriteManager = managers[i];
                
                if (!ShouldProcess(ref spriteManager)) {
                    continue;
                }
                
                this.query.SetSharedComponentFilter(spriteManager);

                AddJob addJob = new AddJob() {
                    chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob),
                    spriteType = spriteType,
                    sortList = spriteManager.SortList
                };

                lastHandle = addJob.Schedule(lastHandle);
            }

            // Sort jobs
            for (int i = 1; i < managers.Count; ++i) {
                SpriteManager spriteManager = managers[i];
                
                if (!ShouldProcess(ref spriteManager)) {
                    continue;
                }

                SortJob sortJob = new SortJob() {
                    sortList = spriteManager.SortList
                };

                lastHandle = sortJob.Schedule(lastHandle);
            }

            // Reset jobs
            for (int i = 1; i < managers.Count; ++i) {
                SpriteManager spriteManager = managers[i];
                
                if (!ShouldProcess(ref spriteManager)) {
                    continue;
                }

                ResetTrianglesJob resetTrianglesJob = new ResetTrianglesJob() {
                    triangles = spriteManager.NativeTriangles
                };

                lastHandle = resetTrianglesJob.Schedule(spriteManager.NativeTriangles.Length,
                    64, lastHandle);
            }

            // Set to triangles jobs
            for (int i = 1; i < managers.Count; ++i) {
                SpriteManager spriteManager = managers[i];
                
                if (!ShouldProcess(ref spriteManager)) {
                    continue;
                }

                SetTrianglesJob setTrianglesJob = new SetTrianglesJob() {
                    triangles = spriteManager.NativeTriangles,
                    sortList = spriteManager.SortList
                };

                lastHandle = setTrianglesJob.Schedule(spriteManager.Count, 64, lastHandle);
            }

            return lastHandle;
        }

        private static bool ShouldProcess(ref SpriteManager manager) {
            return manager.Owner != Entity.Null && (manager.RenderOrderChanged || manager.AlwaysUpdateMesh);
        }

        private void ClearSortLists() {
            // Note that we start from 1 because the first entry is the default
            for (int i = 1; i < this.managerQuery.SharedComponents.Count; ++i) {
                this.managerQuery.SharedComponents[i].SortList.Clear();
            }
        }
        
        [BurstCompile]
        private struct AddJob : IJob {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;

            [ReadOnly]
            public ArchetypeChunkComponentType<Sprite> spriteType;
            
            public NativeList<SortedSpriteEntry> sortList;

            public void Execute() {
                for (int i = 0; i < this.chunks.Length; ++i) {
                    Process(this.chunks[i]);
                }
            }

            private void Process(ArchetypeChunk chunk) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    Sprite sprite = sprites[i];
                    this.sortList.Add(new SortedSpriteEntry(sprite.managerIndex, sprite.LayerOrder,
                        sprite.RenderOrder));
                }
            }
        }

        [BurstCompile]
        private struct SortJob : IJob {
            public NativeList<SortedSpriteEntry> sortList;

            public void Execute() {
                if (this.sortList.Length > 0) {
                    Quicksort(0, this.sortList.Length - 1);
                }
            }

            private void Quicksort(int left, int right) {
                int i = left;
                int j = right;
                SortedSpriteEntry pivot = this.sortList[(left + right) / 2];

                while (i <= j) {
                    // Lesser
                    while (Compare(this.sortList[i], ref pivot) < 0) {
                        ++i;
                    }

                    // Greater
                    while (Compare(this.sortList[j], ref pivot) > 0) {
                        --j;
                    }

                    if (i <= j) {
                        // Swap
                        SortedSpriteEntry temp = this.sortList[i];
                        this.sortList[i] = this.sortList[j];
                        this.sortList[j] = temp;

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

            private int Compare(SortedSpriteEntry a, ref SortedSpriteEntry b) {
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

        // We need this job so that triangle indeces that are past the sorted sprites are erased
        // We do this so that this remainder indeces will not render on top of the sorted ones
        [BurstCompile]
        private struct ResetTrianglesJob : IJobParallelFor {
            public NativeArray<int> triangles;

            public void Execute(int index) {
                this.triangles[index] = 0;
            }
        }

        [BurstCompile]
        private struct SetTrianglesJob : IJobParallelFor {
            [NativeDisableParallelForRestriction]
            public NativeArray<int> triangles;

            [ReadOnly]
            public NativeList<SortedSpriteEntry> sortList;

            public void Execute(int index) {
                if (index >= this.sortList.Length) {
                    return;
                }
                
                // Vertex indeces
                int index1 = this.sortList[index].index * 4;
                int index2 = index1 + 1;
                int index3 = index2 + 1;
                int index4 = index3 + 1;

                // Triangle indeces
                int triangle1 = index * 6;
                int triangle2 = triangle1 + 1;
                int triangle3 = triangle2 + 1;
                int triangle4 = triangle3 + 1;
                int triangle5 = triangle4 + 1;
                int triangle6 = triangle5 + 1;

                // We added this check because the triangles array might not have been expanded yet
                if (triangle1 >= this.triangles.Length) {
                    return;
                }

                // Lower left triangle
                this.triangles[triangle1] = index1;
                this.triangles[triangle2] = index3;
                this.triangles[triangle3] = index2;

                // Upper right triangle
                this.triangles[triangle4] = index3;
                this.triangles[triangle5] = index4;
                this.triangles[triangle6] = index2;
            }
        }
    }
}