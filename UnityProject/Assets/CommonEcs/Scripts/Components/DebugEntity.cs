using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    [GenerateAuthoringComponent]
    public struct DebugEntity : IComponentData {
        public bool isDebug;

        public DebugEntity(bool isDebug) {
            this.isDebug = isDebug;
        }
    }
}
