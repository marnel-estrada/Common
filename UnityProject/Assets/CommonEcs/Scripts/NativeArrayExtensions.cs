using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace CommonEcs {
    public static class NativeArrayExtensions {
        /// <summary>
        /// A faster copy from NativeArray to managed array
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        public static unsafe void CopyToFast<T>(this NativeArray<T> nativeArray, T[] array) where T : struct {
            int byteLength = nativeArray.Length * UnsafeUtility.SizeOf(typeof(T));
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = nativeArray.GetUnsafePtr();
            UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
        }

        public static unsafe void CopyFrom<T>(this NativeArray<T> self, T[] array) where T : struct {
            int byteLength = self.Length * UnsafeUtility.SizeOf(typeof(T));
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = self.GetUnsafePtr();
            UnsafeUtility.MemCpy(nativeBuffer, managedBuffer, byteLength);
        }
        
        public static NativeArray<T> CopyAndExpand<T>(this NativeArray<T> source, int newLength) where T : struct {
            Assert.IsTrue(newLength > source.Length);

            NativeArray<T> newArray = new(newLength, Allocator.Persistent);
            NativeSlice<T> newArraySlice = new(newArray, 0, source.Length);
            NativeSlice<T> originalSlice = new(source);
            newArraySlice.CopyFrom(originalSlice);

            return newArray;
        }
    }
}
