using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A tag component that identifies a signal entity that it has passed a single frame
    /// </summary>
    public struct SignalFramePassed : IComponentData {
    }
}