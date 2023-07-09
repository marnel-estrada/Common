using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class Rendering2dSystemGroup : ComponentSystemGroup {
    }
}