using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A component that can be optionally added so we can optionally run code in systems
    /// that's only used for debugging
    /// </summary>
    public struct DebugFsm : IComponentData {
        public bool isDebug;

        public DebugFsm(bool isDebug) {
            this.isDebug = isDebug;
        }
    }
}