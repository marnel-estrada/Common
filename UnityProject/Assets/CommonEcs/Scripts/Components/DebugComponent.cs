using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    public struct DebugComponent : IComponentData {
        public ByteBool isDebug;
    }
}
