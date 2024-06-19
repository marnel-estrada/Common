using Unity.Collections;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Handles a buffer and the array data of UV indices.
    /// It was done this way since there can be multiple UV indices.
    /// </summary>
    public class UvIndicesBuffer {
        private ComputeBuffer buffer;
        private NativeArray<int> indices;
        private int propertyId;

        public UvIndicesBuffer(string shaderPropertyId, int initialCapacity) {
            this.buffer = new ComputeBuffer(initialCapacity, sizeof(int));
            this.indices = new NativeArray<int>(initialCapacity, Allocator.Persistent);
            this.buffer.SetData(this.indices);

            this.propertyId = Shader.PropertyToID(shaderPropertyId);
        }

        /// <summary>
        /// Sets the buffer to the specified material
        /// </summary>
        /// <param name="material"></param>
        public void SetBuffer(Material material) {
            material.SetBuffer(this.propertyId, this.buffer);
        }
    }
}
