using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A generic IBufferElement that can wrap any struct
    /// </summary>
    [InternalBufferCapacity(10)]
    public struct BufferElement<T> : IBufferElementData where T : struct {
        public T value;

        public BufferElement(T value) {
            this.value = value;
        }
    }
}