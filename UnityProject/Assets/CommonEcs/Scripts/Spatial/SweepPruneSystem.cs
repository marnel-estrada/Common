using CommonEcs;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Common {
    /// <summary>
    /// Handles adding and removal of items into the sweep and prune algorithm.
    /// </summary>
    public partial class SweepPruneSystem : SystemBase {
        private EntityCommandBufferSystem? commandBufferSystem;
        
        private EntityQuery addQuery;
        private EntityQuery removeQuery;
        private EntityQuery updateQuery;

        private SweepPruneContainer container;

        private const int INITIAL_CAPACITY = 8;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            this.addQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SweepPruneEntry>()
                .WithAll<Aabb2Component>()
                .WithNone<Added>().Build(this);

            this.removeQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Added>()
                .WithNone<SweepPruneEntry>()
                .WithNone<Aabb2Component>().Build(this);

            this.updateQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SweepPruneEntry>()
                .WithAll<Aabb2Component>()
                .WithAll<Added>().Build(this);

            this.container = new SweepPruneContainer(INITIAL_CAPACITY, Allocator.Persistent);
        }

        protected override void OnDestroy() {
            this.container.Dispose();
        }

        public ref readonly SweepPruneContainer Container => ref this.container;

        protected override void OnUpdate() {
            if (this.commandBufferSystem == null) {
                throw new CantBeNullException(nameof(this.commandBufferSystem));
            }
            
            EntityTypeHandle entityType = GetEntityTypeHandle();
            ComponentTypeHandle<Aabb2Component> aabb2Type = GetComponentTypeHandle<Aabb2Component>();

            // Can't schedule this in parallel due to usage of SweepPruneContainer
            RemoveJob removeJob = new() {
                entityType = entityType,
                sweepPruneContainer = this.container,
                commandBuffer = this.commandBufferSystem.CreateCommandBuffer()
            };
            this.Dependency = removeJob.Schedule(this.removeQuery, this.Dependency);

            // Can't schedule this in parallel due to usage of SweepPruneContainer
            AddJob addJob = new() {
                entityType = entityType,
                aabb2Type = aabb2Type,
                sweepPruneContainer = this.container,
                commandBuffer = this.commandBufferSystem.CreateCommandBuffer()
            };
            this.Dependency = addJob.Schedule(this.addQuery, this.Dependency);
            
            NativeArray<int> chunkBaseEntityIndices = 
                this.updateQuery.CalculateBaseEntityIndexArray(Allocator.TempJob);
            NativeArray<int> masterListIndices = new(this.updateQuery.CalculateEntityCount(), Allocator.TempJob);
            
            // We collect the master list indices first so we could use it in UpdateJob
            // We can't get SweepPruneItem items from the container map in UpdateJob
            CollectMasterListIndicesJob collectMasterListIndicesJob = new() {
                entityType = entityType,
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                itemMap = this.container.itemMap,
                masterListIndices = masterListIndices
            };
            this.Dependency = collectMasterListIndicesJob.ScheduleParallel(this.updateQuery, this.Dependency);

            UpdateJob updateJob = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                entityType = entityType,
                aabb2Type = aabb2Type,
                masterListIndices = masterListIndices,
                itemMap = this.container.itemMap.AsParallelWriter(),
                masterList = this.container.masterList
            };
            this.Dependency = updateJob.ScheduleParallel(this.updateQuery, this.Dependency);

            SortJob sortJob = new() {
                masterList = this.container.masterList,
                sortedIndices = this.container.sortedIndices
            };
            this.Dependency = sortJob.Schedule(this.Dependency);
            
            // Don't forget to dispose
            this.Dependency = chunkBaseEntityIndices.Dispose(this.Dependency);
            this.Dependency = masterListIndices.Dispose(this.Dependency);
        }
        
        [BurstCompile]
        private struct AddJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;

            [ReadOnly] 
            public ComponentTypeHandle<Aabb2Component> aabb2Type;

            public SweepPruneContainer sweepPruneContainer;

            public EntityCommandBuffer commandBuffer;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<Aabb2Component> boxes = chunk.GetNativeArray(ref this.aabb2Type);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Entity entity = entities[i];
                    this.sweepPruneContainer.Add(entity, boxes[i].WorldBounds);
                    
                    // Add this component so the entity will not be added again
                    this.commandBuffer.AddComponent<Added>(entity);
                }
            }
        }

        [BurstCompile]
        private struct RemoveJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            public SweepPruneContainer sweepPruneContainer;

            public EntityCommandBuffer commandBuffer;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Entity entity = entities[i];
                    this.sweepPruneContainer.Remove(entity);
                    
                    // We remove Added so that the entity would finally be released
                    this.commandBuffer.RemoveComponent<Added>(entity);
                }
            }
        }

        // This collects the master list indeces of SweepPruneItems in a list
        // This is because we can't get this value when we run UpdateJob
        [BurstCompile]
        private struct CollectMasterListIndicesJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;
            
            [ReadOnly]
            public EntityTypeHandle entityType;

            [ReadOnly] 
            public NativeParallelHashMap<Entity, SweepPruneItem> itemMap;

            [NativeDisableParallelForRestriction]
            public NativeArray<int> masterListIndices;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

                ChunkEntityEnumeratorWithQueryIndex enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count,
                    ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    Entity entity = entities[i];
                    this.masterListIndices[queryIndex] = this.itemMap[entity].masterListIndex;
                }
            }
        }

        [BurstCompile]
        private struct UpdateJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;
            
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            [ReadOnly] 
            public ComponentTypeHandle<Aabb2Component> aabb2Type;

            [ReadOnly]
            public NativeArray<int> masterListIndices;
            
            [NativeDisableParallelForRestriction]
            public NativeParallelHashMap<Entity, SweepPruneItem>.ParallelWriter itemMap;
            
            [NativeDisableParallelForRestriction]
            public NativeList<SweepPruneItem> masterList;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<Aabb2Component> boxes = chunk.GetNativeArray(ref this.aabb2Type);

                ChunkEntityEnumeratorWithQueryIndex enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count,
                    ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    Entity entity = entities[i];
                    Aabb2Component box = boxes[i];
                    int masterListIndex = this.masterListIndices[queryIndex];
                    SweepPruneItem updatedItem = new(entity, box.WorldBounds, masterListIndex);

                    // Update item in the map
                    this.itemMap.TryAdd(entity, updatedItem);
                    
                    // Update item in masterList
                    this.masterList[masterListIndex] = updatedItem;
                }
            }
        }
        
        [BurstCompile]
        private struct SortJob : IJob {
            [ReadOnly]
            public NativeList<SweepPruneItem> masterList;

            public NativeList<int> sortedIndices;
            
            public void Execute() {
                SweepPruneComparer comparer = new(this.masterList);
                NativeContainerUtils.InsertionSort(ref this.sortedIndices, comparer);
            }
        }

        // The cleanup component to listen to when adding/removing items from the algorithm
        private readonly struct Added : ICleanupComponentData {
        }
    }
}