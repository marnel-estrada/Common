using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A tag component that identifies as Entity as for removal
    /// There may be systems in which the way to remove something is to add this component instead
    /// </summary>
    public struct ForRemoval : IComponentData {
    }
}