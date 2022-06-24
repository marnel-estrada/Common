using System;

using Unity.Collections;

namespace CommonEcs {
    public static class NativeMultiHashMapExtensions {
        /// <summary>
        /// Returns an array of values instead of enumerator. Client code is responsible for deallocating
        /// the array if it is non temp.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="allocator"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static NativeArray<V> GetValuesAsArray<K, V>(this ref NativeParallelMultiHashMap<K, V> self, in K key, Allocator allocator)
            where K : struct, IEquatable<K> 
            where V : struct {
            NativeParallelMultiHashMap<K, V>.Enumerator values = self.GetValuesForKey(key);
            NativeArray<V> array = new NativeArray<V>(self.CountValuesForKey(key), allocator);
            
            // Copy to array
            int index = 0;
            while (values.MoveNext()) {
                array[index] = values.Current;
                ++index;
            }

            return array;
        }
    }
}