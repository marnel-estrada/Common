using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Note that this is a composed system that is to be used in an ISystem struct
    /// So this struct does not implement ISystem so it won't be picked up by the system
    /// instantiations.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <typeparam name="TProcessor"></typeparam>
    /// <typeparam name="TStrategy"></typeparam>
    public struct ConsiderationISystem<TComponent, TProcessor, TStrategy> 
        where TComponent : unmanaged, IConsiderationComponent
        where TProcessor : unmanaged, IConsiderationProcess<TComponent> 
        where TStrategy : unmanaged, IConsiderationSystemStrategy<TComponent, TProcessor> {
        private EntityQuery query;
        private bool isFilterZeroSized;
        private TStrategy strategy;
        
        // Handles
        private ComponentTypeHandle<Consideration> considerationType;
        private ComponentTypeHandle<TComponent> filterComponentType;
        
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Consideration>()
                .WithAll<TComponent>()
                .Build(ref state);
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TComponent>()).IsZeroSized;
            
            this.strategy.OnCreate(ref state);

            this.considerationType = state.GetComponentTypeHandle<Consideration>();
            this.filterComponentType = state.GetComponentTypeHandle<TComponent>();
        }

        public void OnDestroy(ref SystemState state) {
            this.strategy.OnDestroy(ref state);
        }

        public void OnUpdate(ref SystemState state) {
            if (!this.strategy.CanExecute(ref state)) {
                // Can't execute based on some logic. The strategy might be waiting for something.
                return;
            }
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(
                state.WorldUpdateAllocator, state.Dependency, out JobHandle chunkBaseIndicesHandle);
            state.Dependency = JobHandle.CombineDependencies(state.Dependency, chunkBaseIndicesHandle);
            
            // Update handles
            this.considerationType.Update(ref state);
            this.filterComponentType.Update(ref state);
            
            ComputeConsiderationsJob job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                considerationType = this.considerationType,
                filterType = this.filterComponentType,
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = this.strategy.CreateProcess(ref state)
            };
            
            state.Dependency = this.strategy.ShouldScheduleParallel ? job.ScheduleParallel(this.query, state.Dependency) : 
                job.Schedule(this.query, state.Dependency);
            
            // Don't forget to dispose
            state.Dependency = chunkBaseEntityIndices.Dispose(state.Dependency);
        }
        
        [BurstCompile]
        public struct ComputeConsiderationsJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;

            public ComponentTypeHandle<Consideration> considerationType;

            public ComponentTypeHandle<TComponent> filterType;

            public bool filterHasArray;
            public TProcessor processor;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Consideration> considerations = chunk.GetNativeArray(ref this.considerationType);
                
                NativeArray<TComponent> filters = this.filterHasArray ? chunk.GetNativeArray(ref this.filterType) : default;
                TComponent defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                this.processor.BeforeChunkIteration(chunk);

                ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                    useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    Consideration consideration = considerations[i];
                    if (!consideration.shouldExecute) {
                        // Not time to execute yet
                        continue;
                    }

                    // Compute the utility
                    if (this.filterHasArray) {
                        // Use filter component if it has data
                        TComponent filter = filters[i];
                        consideration.value = this.processor.ComputeUtility(consideration.agentEntity, filter, i, queryIndex);
                    } else {
                        // Filter has no component. Just use default data.
                        consideration.value = this.processor.ComputeUtility(consideration.agentEntity, defaultFilter, i, queryIndex);
                    }
                    
                    // Modify
                    considerations[i] = consideration;
                }
            }
        }
    }
}