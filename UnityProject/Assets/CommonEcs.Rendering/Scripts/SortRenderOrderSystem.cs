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
                int count = this.query.CalculateEntityCount();
                NativeArray<SortedSpriteEntry> entries = new NativeArray<SortedSpriteEntry>(count, Allocator.TempJob);

                AddJob addJob = new AddJob() {
                    spriteType = spriteType,
                    sortList = entries
                };

                lastHandle = addJob.Schedule(this.query, lastHandle);
                
                // Sort
                lastHandle = MultiThreadSort(entries, lastHandle);
                
                ResetTrianglesJob resetTrianglesJob = new ResetTrianglesJob() {
                    triangles = spriteManager.NativeTriangles
                };

                lastHandle = resetTrianglesJob.Schedule(spriteManager.NativeTriangles.Length,
                    64, lastHandle);
                
                SetTrianglesJob setTrianglesJob = new SetTrianglesJob() {
                    triangles = spriteManager.NativeTriangles,
                    sortList = entries
                };

                lastHandle = setTrianglesJob.Schedule(spriteManager.Count, 64, lastHandle);
                
                // Don't forget to deallocate the array
                lastHandle = new DeallocateNativeArrayJob<SortedSpriteEntry>() {
                    array = entries
                }.Schedule(lastHandle);
            }

            return lastHandle;
        }

        private static bool ShouldProcess(ref SpriteManager manager) {
            return manager.Owner != Entity.Null && (manager.RenderOrderChanged || manager.AlwaysUpdateMesh);
        }
        
        [BurstCompile]
        private struct AddJob : IJobChunk {
            [ReadOnly]
            public ArchetypeChunkComponentType<Sprite> spriteType;
            
            public NativeArray<SortedSpriteEntry> sortList;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    Sprite sprite = sprites[i];
                    int index = firstEntityIndex + i;
                    this.sortList[index] = new SortedSpriteEntry(sprite.managerIndex, sprite.LayerOrder,
                        sprite.RenderOrder);
                }
            }
        }
        
        // This is copied from MultithreadedSort so that it will be Burst compiled on build
        private static JobHandle MultiThreadSort(NativeArray<SortedSpriteEntry> array, JobHandle parentHandle) {
            return Sort(array, new MultithreadedSort.SortRange(0, array.Length - 1), parentHandle);
        }

        private static JobHandle Sort(NativeArray<SortedSpriteEntry> array, MultithreadedSort.SortRange range,
            JobHandle parentHandle) {
            if (range.Length <= MultithreadedSort.SINGLE_THREAD_THRESHOLD_LENGTH) {
                // Use single threaded sort
                return new MultithreadedSort.SingleThreadSortJob<SortedSpriteEntry>() {
                    array = array, left = range.left, right = range.right
                }.Schedule(parentHandle);
            }

            int middle = range.Middle;

            MultithreadedSort.SortRange left = new MultithreadedSort.SortRange(range.left, middle);
            JobHandle leftHandle = Sort(array, left, parentHandle);

            MultithreadedSort.SortRange right = new MultithreadedSort.SortRange(middle + 1, range.right);
            JobHandle rightHandle = Sort(array, right, parentHandle);

            JobHandle combined = JobHandle.CombineDependencies(leftHandle, rightHandle);

            return new MultithreadedSort.Merge<SortedSpriteEntry>() {
                array = array, first = left, second = right
            }.Schedule(combined);
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
            public NativeArray<SortedSpriteEntry> sortList;

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