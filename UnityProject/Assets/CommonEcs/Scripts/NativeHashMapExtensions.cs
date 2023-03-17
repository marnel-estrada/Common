using System;

using Unity.Collections;

namespace CommonEcs {
    public static class NativeHashMapExtensions {
        /// <summary>
        /// Adds or replaces a hashmap entry
        /// </summary>
        /// <param name="hashMap"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void AddOrReplace<TKey, TValue>(this NativeParallelHashMap<TKey, TValue> hashMap, TKey key, TValue value)
            where TKey : unmanaged, IEquatable<TKey> 
            where TValue : unmanaged {
            hashMap.Remove(key);
            hashMap.TryAdd(key, value);
        }
    }
}