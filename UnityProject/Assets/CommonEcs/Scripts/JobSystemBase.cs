using Common;

using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// An abstract base class for systems that use jobs.
    /// This was primarily created to make JobComponentSystem change into SystemBase easier.
    /// </summary>
    public abstract partial class JobSystemBase : SystemBase {
        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        protected abstract JobHandle OnUpdate(JobHandle inputDeps);

        protected T GetOrCreateSystem<T>() where T : ComponentSystemBase {
            T system = this.World.GetOrCreateSystem<T>();
            Assertion.NotNull(system);
            return system;
        }
    }
}