using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    public class ResolveBestOptionSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(UtilityBrain));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}