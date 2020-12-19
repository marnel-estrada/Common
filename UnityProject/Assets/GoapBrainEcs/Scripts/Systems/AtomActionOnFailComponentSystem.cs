using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A generic system for composing on fail routines of atom actions
    /// T here is the AtomAction component type
    /// </summary>
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    public abstract class AtomActionOnFailComponentSystem<T> : ComponentSystem where T : struct, IComponentData {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomActionOnFail), typeof(T));
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach(delegate(ref AtomActionOnFail onFail, ref T actionComponent) {
                OnFail(ref onFail, ref actionComponent);
                onFail.done = true;
            });
        }

        protected abstract void OnFail(ref AtomActionOnFail onFail, ref T actionComponent);
    }
}