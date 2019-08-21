using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// A signal handler that is implemented as a JobComponentSystem
    /// </summary>
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerJobComponentSystem<T> : JobComponentSystem where T : struct, IComponentData {
        private EntityQuery signalQuery;
        private JobSignalHandler<T> signalHandler;

        private EndPresentationEntityCommandBufferSystem barrier;
        
        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(T), ComponentType.Exclude<ProcessedBySystem>());
            this.signalHandler = new JobSignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);

            this.barrier = this.World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();
        }

        protected abstract JobHandle OnDispatch(Entity entity, T signalComponent, JobHandle inputDeps);

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            JobHandle handle = this.signalHandler.Update(inputDeps);
            
            // We added this component so that they will not be processed again 
            this.barrier.CreateCommandBuffer().AddComponent(this.signalQuery, typeof(ProcessedBySystem));

            return handle;
        }
    }
}