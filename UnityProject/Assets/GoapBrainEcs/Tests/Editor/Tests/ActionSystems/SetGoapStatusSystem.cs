using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public class SetGoapStatusSystem : AtomActionComponentSystem<SetGoapStatus> {
        protected override GoapStatus Start(ref AtomAction atomAction, ref SetGoapStatus actionComponent) {
            return actionComponent.status;
        }
    }
}