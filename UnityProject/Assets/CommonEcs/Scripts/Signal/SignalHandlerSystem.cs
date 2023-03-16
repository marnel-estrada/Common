using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// This is used for signal handler systems that responds to the same signal parameter
    /// We used recursive generic type so that there's a unique ProcessedBySystem for each
    /// system that derives from this class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerSystem<T, S> : SystemBase where T : unmanaged, IComponentData
        where S : SignalHandlerSystem<T, S> {
        private SignalHandler<T> signalHandler;
        private EntityQuery signalQuery;

        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), 
                typeof(T), 
                ComponentType.Exclude<ProcessedBySystem>());
            
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);

            this.commandBufferSystem = this.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected abstract void OnDispatch(Entity entity, T parameter);

        protected override void OnUpdate() {
            this.signalHandler.Update();
            
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            commandBuffer.AddComponent<ProcessedBySystem>(this.signalQuery);
        }

        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }
    }
}