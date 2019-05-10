using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(EndPresentationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DestroySignalsSystem : ComponentSystem {
        private EntityQuery query;
    
        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(Signal));
        }
    
        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}