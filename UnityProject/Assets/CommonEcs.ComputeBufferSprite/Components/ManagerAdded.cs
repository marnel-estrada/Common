using Unity.Entities;

namespace CommonEcs {
    // A cleanup component that we use to identify that the sprite has already
    // been added
    internal readonly struct ManagerAdded : ICleanupComponentData {
        // We keep the manager index so we know which index to remove on sprite removal
        public readonly int managerIndex;

        public ManagerAdded(int managerIndex) {
            this.managerIndex = managerIndex;
        }
    }
}