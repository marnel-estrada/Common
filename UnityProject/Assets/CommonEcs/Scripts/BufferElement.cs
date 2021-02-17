using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A generic IBufferElement that can wrap any struct
    /// </summary>
    [InternalBufferCapacity(32)]
    public readonly struct BufferElement<T> : IBufferElementData where T : struct {
        public readonly T value;
    
        public BufferElement(T value) {
            this.value = value;
        }
    }
}