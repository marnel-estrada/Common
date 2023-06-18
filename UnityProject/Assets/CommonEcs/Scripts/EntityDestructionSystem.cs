using Unity.Entities;

namespace CommonEcs {
    public partial class EntityDestructionSystem : SystemBase {
        private EntityQuery query;
        
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ForDestruction));
            
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            this.commandBufferSystem.CreateCommandBuffer().DestroyEntity(this.query);
            this.commandBufferSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}