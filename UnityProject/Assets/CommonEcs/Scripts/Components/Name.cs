using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common component for named entities
    /// </summary>
    public readonly struct Name : IComponentData {
        public readonly FixedString64 value;

        public Name(FixedString64 value) {
            this.value = value;
        }
    }
}