using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial class Rendering2dSystemGroup : ComponentSystemGroup {
    }
}