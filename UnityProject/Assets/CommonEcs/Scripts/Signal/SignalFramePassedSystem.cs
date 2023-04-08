using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(DestroySignalsSystem))]
    public class SignalFramePassedSystem : SystemBase {
        private EntityQuery query;

        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Signal), 
                ComponentType.Exclude<SignalFramePassed>());

            this.commandBufferSystem = this.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();
            commandBuffer.AddComponent<SignalFramePassed>(this.query);
        }
    }
}