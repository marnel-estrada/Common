using Unity.Entities;

namespace CommonEcs {
    public partial class EntityDestructionSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ForDestruction));
        }

        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}