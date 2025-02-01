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
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(Allocator.TempJob);
            
            ComputeConsiderationsJob<TComponent, TProcessor> job = new() {
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
    }
}