using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// System group for CommonEcs.Goap systems
    /// </summary>
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public partial class GoapSystemGroup : ComponentSystemGroup {
    }
}