using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A component that can be added to entities so it's easier to look for them
    /// </summary>
    public struct Marker : IComponentData {
        public readonly FixedString64Bytes label;

        public Marker(FixedString64Bytes label) {
            this.label = label;
        }
    }
}
