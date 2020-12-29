using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// An abstract base class for systems that use jobs.
    /// This was primarily created to make JobComponentSystem change into SystemBase easier.
    /// </summary>
    public abstract class JobSystemBase : SystemBase {
        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        protected abstract JobHandle OnUpdate(JobHandle inputDeps);
    }
}