using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public class RemoveStartStateSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmState), typeof(StartState));
        }

        protected override void OnUpdate() {
            this.EntityManager.RemoveComponent<StartState>(this.query);
        }
    }
}