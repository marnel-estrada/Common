using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// A struct that wraps an int2 for use in DynamicBuffer
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct Int2BufferElement : IBufferElementData {
        public int2 value;

        public Int2BufferElement(int2 value) {
            this.value = value;
        }
    }
}