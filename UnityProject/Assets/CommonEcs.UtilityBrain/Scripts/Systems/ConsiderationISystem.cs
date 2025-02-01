using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public struct ConsiderationISystem<TComponent, TProcessor, TStrategy> 
        where TComponent : unmanaged, IConsiderationComponent
        where TProcessor : unmanaged, IConsiderationProcess<TComponent> 
        where TStrategy : unmanaged, IConsiderationSystemStrategy<TComponent, TProcessor> {
        private EntityQuery query;
        private bool isFilterZeroSized;
        private TStrategy strategy;
        
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Consideration>()
                .WithAll<TComponent>()
                .Build(ref state);
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TComponent>()).IsZeroSized;
            
            this.strategy.OnCreate(ref state);
        }

        public void OnDestroy(ref SystemState state) {
            this.strategy.OnDestroy(ref state);
        }

        public void OnUpdate(ref SystemState state) {
            if (!this.strategy.CanExecute(ref state)) {
                // Can't execute based on some logic. The strategy might be waiting for something.
                return;
            }
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(Allocator.TempJob);
            
            ComputeConsiderationsJob<TComponent, TProcessor> job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                considerationType = state.GetComponentTypeHandle<Consideration>(),
                filterType = state.GetComponentTypeHandle<TComponent>(),
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = this.strategy.CreateProcess(ref state)
            };
            
            state.Dependency = this.strategy.ShouldScheduleParallel ? job.ScheduleParallel(this.query, state.Dependency) : 
                job.Schedule(this.query, state.Dependency);
            
            // Don't forget to dispose
            state.Dependency = chunkBaseEntityIndices.Dispose(state.Dependency);
        }
    }
}