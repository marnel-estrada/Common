using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Common {
    /// <summary>
    /// An array stored in the unmanaged heap
    /// http://JacksonDunstan.com/articles/3740
    /// </summary>
    public unsafe struct UnmanagedArray {
        /// <summary>
        /// Number of elements in the array
        /// </summary>
        public int Length;

        /// <summary>
        /// The size of one element of the array in bytes
        /// </summary>
        public int ElementSize;

        /// <summary>
        /// Pointer to the unmanaged heap memory the array is stored in
        /// </summary>
        public void* Memory;

        /// <summary>
        /// Create the array. Its elements are initially undefined.
        /// </summary>
        /// <param name="length">Number of elements in the array</param>
        /// <param name="elementSize">The size of one element of the array in bytes</param>
        public UnmanagedArray(int length, int elementSize) {
            Memory = (void*)UnmanagedMemory.Alloc(length * elementSize);
            Length = length;
            ElementSize = elementSize;
        }

        /// <summary>
        /// Get a pointer to an element in the array
        /// </summary>
        /// <param name="index">Index of the element to get a pointer to</param>
        public void* this[int index] {
            get {
                return ((byte*)Memory) + ElementSize * index;
            }
        }

        /// <summary>
        /// Free the unmanaged heap memory where the array is stored, set <see cref="Memory"/> to null,
        /// and <see cref="Length"/> to zero.
        /// </summary>
        public void Destroy() {
            UnmanagedMemory.Free((IntPtr)Memory);
            Memory = null;
            Length = 0;
        }

    }
}
