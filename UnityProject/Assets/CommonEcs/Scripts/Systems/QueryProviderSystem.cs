using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common system that can provide entity queries
    /// This was made so that the managed side can run jobs like IJobEntityBatch
    /// </summary>
    [DisableAutoCreation]
    public partial class QueryProviderSystem : SystemBase {
        protected override void OnCreate() {
            base.OnCreate();

            // Need not run per frame
            this.Enabled = false;
        }

        public EntityQuery GetQuery(params ComponentType[] componentTypes) {
            return GetEntityQuery(componentTypes);
        }

        protected override void OnUpdate() {
        }
    }
}