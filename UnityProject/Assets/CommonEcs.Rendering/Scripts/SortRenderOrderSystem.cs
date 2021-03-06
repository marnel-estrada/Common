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
    public partial class SortRenderOrderSystem : JobSystemBase {
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
            ComponentTypeHandle<Sprite> spriteType = GetComponentTypeHandle<Sprite>(true);

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

                lastHandle = addJob.ScheduleParallel(this.query, lastHandle);
                
                // Sort
                lastHandle = MultithreadedSort.Sort(entries, lastHandle);
                
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
                lastHandle = entries.Dispose(lastHandle);
            }

            return lastHandle;
        }

        private static bool ShouldProcess(ref SpriteManager manager) {
            return manager.Owner != Entity.Null && (manager.RenderOrderChanged || manager.AlwaysUpdateMesh);
        }
        
        [BurstCompile]
        private struct AddJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<Sprite> spriteType;
            
            public NativeArray<SortedSpriteEntry> sortList;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    Sprite sprite = sprites[i];
                    int index = firstEntityIndex + i;
                    this.sortList[index] = new SortedSpriteEntry(sprite.managerIndex, sprite.LayerOrder,
                        sprite.renderOrder, sprite.renderOrderDueToPosition);
                }
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