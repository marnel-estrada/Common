using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace Common {
    /// <summary>
    /// Allocates and frees blocks of unmanaged memory. Tracks allocations in the Unity editor so they
    /// can be freed in bulk via <see cref="Cleanup"/>.
    /// http://JacksonDunstan.com/articles/3740
    /// </summary>
    public static class UnmanagedMemory {
        /// <summary>
        /// Keep track of all the allocations that haven't been freed
        /// </summary>
#if UNITY_EDITOR
        private static readonly HashSet<IntPtr> allocations = new HashSet<IntPtr>();
#endif

        /// <summary>
        /// Allocate unmanaged heap memory and track it
        /// </summary>
        /// <param name="size">Number of bytes of unmanaged heap memory to allocate</param>
        public static IntPtr Alloc(int size) {
            var ptr = Marshal.AllocHGlobal(size);
#if UNITY_EDITOR
            allocations.Add(ptr);
#endif
            return ptr;
        }

        /// <summary>
        /// Free unmanaged heap memory and stop tracking it
        /// </summary>
        /// <param name="ptr">Pointer to the unmanaged heap memory to free</param>
        public static void Free(IntPtr ptr) {
            Marshal.FreeHGlobal(ptr);
#if UNITY_EDITOR
            allocations.Remove(ptr);
#endif
        }

        /// <summary>
        /// Free all unmanaged heap memory allocated with <see cref="Alloc"/>
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Cleanup() {
#if UNITY_EDITOR
            foreach (var ptr in allocations) {
                Marshal.FreeHGlobal(ptr);
            }
            allocations.Clear();
#endif
        }
    }
}
