using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class EntityDestructionSystem : ComponentSystem {
        private EntityQuery query;
        private EntityTypeHandle entityType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ForDestruction));
        }

        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}