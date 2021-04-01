using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Moves the agent's atomActionIndex or actionIndex.
    /// </summary>
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    public class MoveToNextActionSystem : JobSystemBase {

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}