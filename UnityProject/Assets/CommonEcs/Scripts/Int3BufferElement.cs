using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// A struct that wraps an int3 for use in DynamicBuffer
    /// </summary>
    public readonly struct Int3BufferElement : IBufferElementData {
        public readonly int3 value;

        public Int3BufferElement(int3 value) {
            this.value = value;
        }
    }
}