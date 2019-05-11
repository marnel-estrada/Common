using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class EntityDestructionSystem : ComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkEntityType entityType;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(ForDestruction));
        }

        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}