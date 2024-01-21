using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class Rendering2dSystemGroup : ComponentSystemGroup {
    }
}