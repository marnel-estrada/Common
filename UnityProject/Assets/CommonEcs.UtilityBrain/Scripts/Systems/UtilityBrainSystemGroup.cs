using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public partial class UtilityBrainSystemGroup : ComponentSystemGroup {
    }
}