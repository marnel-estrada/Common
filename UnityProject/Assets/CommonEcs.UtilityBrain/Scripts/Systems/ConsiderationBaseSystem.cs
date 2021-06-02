using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    [UpdateInGroup(typeof(UtilityBrainSystemGroup))]
    [UpdateAfter(typeof(IdentifyOptionsAndConsiderationsToExecuteSystem))]
    [UpdateBefore(typeof(WriteValuesToOwnersSystem))]
    public abstract class ConsiderationBaseSystem<TFilter, TProcessor> : JobSystemBase
        where TFilter : unmanaged, IConsiderationComponent
        where TProcessor : struct, IConsiderationProcess<TFilter> {
        private EntityQuery query;
        private bool isFilterZeroSized;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Consideration), typeof(TFilter));
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TFilter>()).IsZeroSized;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                considerationType = GetComponentTypeHandle<Consideration>(),
                filterType = GetComponentTypeHandle<TFilter>(),
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = PrepareProcessor()
            };
            
            return this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, 1, inputDeps) : 
                job.Schedule(this.query, inputDeps);
        }
        
        /// <summary>
        /// There may be times that the action system might not want to schedule in parallel
        /// Like for cases when they write using ComponentDataFromEntity
        /// </summary>
        protected virtual bool ShouldScheduleParallel {
            get {
                return true;
            }
        }
        
        protected abstract TProcessor PrepareProcessor();
        
        [BurstCompile]
        public struct Job : IJobEntityBatchWithIndex {
            public ComponentTypeHandle<Consideration> considerationType;
            public ComponentTypeHandle<TFilter> filterType;
            public bool filterHasArray;
            public TProcessor processor;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<Consideration> considerations = batchInChunk.GetNativeArray(this.considerationType);
                
                NativeArray<TFilter> filters = this.filterHasArray ? batchInChunk.GetNativeArray(this.filterType) : default;
                TFilter defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                this.processor.BeforeChunkIteration(batchInChunk, batchIndex);
                
                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    Consideration consideration = considerations[i];
                    if (!consideration.shouldExecute) {
                        // Not time to execute yet
                        continue;
                    }

                    // Compute the utility
                    if (this.filterHasArray) {
                        // Use filter component if it has data
                        TFilter filter = filters[i];
                        consideration.value = this.processor.ComputeUtility(consideration.agentEntity, filter, indexOfFirstEntityInQuery, i);
                    } else {
                        // Filter has no component. Just use default data.
                        consideration.value = this.processor.ComputeUtility(consideration.agentEntity, defaultFilter, indexOfFirstEntityInQuery, i);
                    }
                    
                    // Modify
                    considerations[i] = consideration;
                }
            }
        }
    }
}