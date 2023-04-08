using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(DestroySignalsSystem))]
    public abstract class SignalHandlerComponentSystem<T> : SystemBase where T : unmanaged, IComponentData {
        private EntityQuery signalQuery;
        private SignalHandler<T> signalHandler;

        private EntityCommandBufferSystem commandBufferSystem;

        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), 
                typeof(T), 
                ComponentType.Exclude<ProcessedBySystem>());
            
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);

            this.commandBufferSystem = this.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected abstract void OnDispatch(Entity entity, T signalComponent);

        protected override void OnUpdate() {
            this.signalHandler.Update();
            
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            commandBuffer.AddComponent<ProcessedBySystem>(this.signalQuery);
        }
    }
}