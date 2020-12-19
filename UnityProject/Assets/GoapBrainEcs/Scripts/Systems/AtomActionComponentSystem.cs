using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// T here is the AtomAction component type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public abstract class AtomActionComponentSystem<T> : ComponentSystem where T : struct, IComponentData {
        private EntityQuery query;
        private EntityQueryBuilder.F_DD<AtomAction, T> updateForEach;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction), typeof(T));
            this.updateForEach = delegate(ref AtomAction atomAction, ref T actionComponent) {
                // Start
                if (!atomAction.started) {
                    atomAction.status = Start(ref atomAction, ref actionComponent);
                    if (atomAction.status == GoapStatus.SUCCESS || atomAction.status == GoapStatus.FAILED) {
                        // Action is already done
                        return;
                    }
                }
                
                // Update
                atomAction.status = Update(ref atomAction, ref actionComponent);
            };
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach(this.updateForEach);
        }

        protected virtual GoapStatus Start(ref AtomAction atomAction, ref T actionComponent) {
            return GoapStatus.SUCCESS;
        }

        protected virtual GoapStatus Update(ref AtomAction atomAction, ref T actionComponent) {
            return GoapStatus.SUCCESS;
        } 
    }
}