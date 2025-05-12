using System.Collections.Generic;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CommonEcs {
    [UpdateBefore(typeof(UpdateChangedVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateBefore(typeof(SpriteManagerJobsFinisher))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class SortRenderOrderSystem : JobSystemBase {
        private EntityQuery query;

        private SharedComponentQuery<SpriteManager> managerQuery;

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<Sprite>(), ComponentType.ReadOnly<SpriteManager>());
            RequireForUpdate(this.query);
            
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
                
                this.query.SetSharedComponentFilterManaged(spriteManager);
                int count = this.query.CalculateEntityCount();
                NativeArray<SortedSpriteEntry> entries = CollectionHelper.CreateNativeArray<SortedSpriteEntry>(count, WorldUpdateAllocator);
                
                NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(
                    WorldUpdateAllocator, inputDeps, out JobHandle chunkBaseIndicesHandle);
                inputDeps = JobHandle.CombineDependencies(inputDeps, chunkBaseIndicesHandle);
                
                AddJob addJob = new() {
                    chunkBaseEntityIndices = chunkBaseEntityIndices,
                    spriteType = spriteType,
                    sortList = entries
                };


                lastHandle = addJob.ScheduleParallel(this.query, lastHandle);
                
                // Sort
                lastHandle = MultithreadedSort.Sort(entries, lastHandle);
                
                ResetTrianglesJob resetTrianglesJob = new() {
                    triangles = spriteManager.NativeTriangles
                };

                lastHandle = resetTrianglesJob.Schedule(spriteManager.NativeTriangles.Length,
                    64, lastHandle);
                
                SetTrianglesJob setTrianglesJob = new () {
                    triangles = spriteManager.NativeTriangles,
                    sortList = entries
                };

                lastHandle = setTrianglesJob.Schedule(spriteManager.Count, 64, lastHandle);
                
                // Don't forget to deallocate the arrays
                lastHandle = chunkBaseEntityIndices.Dispose(lastHandle);
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
            public NativeArray<int> chunkBaseEntityIndices;

            [ReadOnly]
            public ComponentTypeHandle<Sprite> spriteType;

            [NativeDisableParallelForRestriction]
            public NativeArray<SortedSpriteEntry> sortList;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);

                ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                    useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    Sprite sprite = sprites[i];
                    this.sortList[queryIndex] = new SortedSpriteEntry(sprite.managerIndex, sprite.LayerOrder,
                        sprite.renderOrder, sprite.renderOrderDueToPosition);
                }
            }
        }

        // We need this job so that triangle indeces that are past the sorted sprites are erased
        // We do this so that this remainder indeces will not render on top of the sorted ones
        [BurstCompile]
        private struct ResetTrianglesJob : IJobParallelFor {
            [NativeDisableParallelForRestriction]
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