using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    [UpdateInGroup(typeof(UtilityBrainSystemGroup))]
    [UpdateAfter(typeof(IdentifyOptionsAndConsiderationsToExecuteSystem))]
    [UpdateBefore(typeof(WriteValuesToOwnersSystem))]
    public abstract partial class ConsiderationBaseSystem<TComponent, TProcessor> : JobSystemBase
        where TComponent : unmanaged, IConsiderationComponent
        where TProcessor : unmanaged, IConsiderationProcess<TComponent> {
        private EntityQuery query;
        private bool isFilterZeroSized;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Consideration), typeof(TComponent));
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TComponent>()).IsZeroSized;
        }

        protected virtual bool CanExecute => true;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (!this.CanExecute) {
                // Can't execute based on some logic. The deriving system might be waiting for something.
                return inputDeps;
            }
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(
                WorldUpdateAllocator, inputDeps, out JobHandle chunkBaseIndicesHandle);
            inputDeps = JobHandle.CombineDependencies(inputDeps, chunkBaseIndicesHandle);
            
            ComputeConsiderationsJob job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                considerationType = GetComponentTypeHandle<Consideration>(),
                filterType = GetComponentTypeHandle<TComponent>(),
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = PrepareProcessor()
            };
            
            JobHandle handle = this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, inputDeps) : 
                job.Schedule(this.query, inputDeps);
            
            // Don't forget to dispose
            handle = chunkBaseEntityIndices.Dispose(handle);

            return handle;
        }
        
        /// <summary>
        /// There may be times that the action system might not want to schedule in parallel
        /// Like for cases when they write using ComponentLookup
        /// </summary>
        protected virtual bool ShouldScheduleParallel => true;

        protected abstract TProcessor PrepareProcessor();
        
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