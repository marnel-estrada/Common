using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public class UtilityBrainSystemGroup : ComponentSystemGroup {
    }
}