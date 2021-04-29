using System;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A hashmap that uses a DynamicBuffer as its buckets
    /// This is useful for cases where the elements are child entities that wants to write
    /// to its parent hashmap. Instead of writing directly to a hashmap, they write to an
    /// element in the DynamicBuffer. This way, the child elements can be run in parallel.
    /// </summary>
    public struct DynamicBufferHashMap<K, V> : IComponentData
        where K : unmanaged, IEquatable<K>
        where V : unmanaged, IEquatable<V> {
        private int count;

        public const int MAX_COUNT = 64;

        /// <summary>
        /// This should be called prior to usage so that all of the slots are prepared.
        /// </summary>
        /// <param name="bucket"></param>
        public static void Init(ref DynamicBuffer<Entry<V>> bucket) {
            bucket.EnsureCapacity(MAX_COUNT);
            for (int i = 0; i < MAX_COUNT; ++i) {
                bucket.Add(Entry<V>.Nothing);
            }
        }

        /// <summary>
        /// Adds or sets a value to the specified key
        /// Returns the bucket index where the value is stored
        /// This could be used by child entities so they know where to write their data.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int AddOrSet(ref DynamicBuffer<Entry<V>> bucket, in K key, in V value) {
            if (this.count == MAX_COUNT) {
                throw new Exception("Bucket is full");
            }
            
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);

            if (!bucket[bucketIndex].HasValue) {
                // This means that it's an empty slot. We can place the value here.
                bucket[bucketIndex] = Entry<V>.Something(hashCode, value);
                ++this.count;
                return bucketIndex;
            }
            
            // At this point, there's an existing value at the slot
            // Let's check if they have the same hashcode
            if (bucket[bucketIndex].HashCode == hashCode) {
                // It's the same item. Let's replace the value.
                bucket[bucketIndex] = Entry<V>.Something(hashCode, value);
                ++this.count;
                return bucketIndex;
            }
            
            // At this point, the item at the resolved index is a different item
            // Let's do linear probing
            // Note here that we start on the next index because bucketIndex is already checked
            ValueTypeOption<int> probedIndex = LinearProbeForAdding(bucket, hashCode, bucketIndex + 1);
            if(probedIndex.IsSome) {
                // We pass -1 here to cause an array index exception if probedIndex turned out to be a None
                int probedIndexValue = probedIndex.ValueOr(-1);
                bucket[probedIndexValue] = Entry<V>.Something(hashCode, value);
                ++this.count;

                return probedIndexValue;
            }
            
            // At this point, the returned probed index is a none
            // This means that the bucket is full.
            throw new Exception("Bucket is full");
        }
        
        /// <summary>
        /// Look for a bucket index that's empty or of the same hashcode
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="hashCode"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private ValueTypeOption<int> LinearProbeForAdding(in DynamicBuffer<Entry<V>> bucket, int hashCode, int startingIndex) {
            for (int i = 0; i < MAX_COUNT; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                // Using bitwise operator is faster instead of using modulo
                int checkIndex = (startingIndex + i) & ~MAX_COUNT;
                
                Entry<V> entry = bucket[checkIndex];
                if (!entry.HasValue || entry.HashCode == hashCode) {
                    return ValueTypeOption<int>.Some(checkIndex);
                }
            }
            
            // We've checked all entries. We can't find a suitable slot.
            return ValueTypeOption<int>.None;
        }

        public void Remove(ref DynamicBuffer<Entry<V>> bucket, in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check of the slot at the resolve index is already the item
            Entry<V> entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // This is the item. We remove it.
                bucket[bucketIndex] = Entry<V>.Nothing;
                --this.count;
                return;
            }
            
            // At this point, the entry at the resolved index is a different item
            // Let's linear probe for the item
            // We pass true as we only check slots with values.
            ValueTypeOption<int> probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);
            if (probedIndex.IsSome) {
                // We found the item
                int probedIndexValue = probedIndex.ValueOr(-1);
                bucket[probedIndexValue] = Entry<V>.Nothing;
                --this.count;
            }
            
            // When probedIndex is None, it just means that the item is not in the HashMap
        }

        public readonly ValueTypeOption<V> Find(in DynamicBuffer<Entry<V>> bucket, in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check if key is already at the resolved index
            Entry<V> entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // Found the item
                return ValueTypeOption<V>.Some(entry.Value);
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            ValueTypeOption<int> probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);
            if (probedIndex.IsSome) {
                int probedIndexValue = probedIndex.ValueOr(-1);
                return ValueTypeOption<V>.Some(bucket[probedIndexValue].Value);
            }
            
            // At this point, we didn't find the item
            return ValueTypeOption<V>.None;
        }

        public bool Contains(in DynamicBuffer<Entry<V>> bucket, in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check if key is already at the resolved index
            Entry<V> entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // Found the item
                return true;
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            ValueTypeOption<int> probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);

            // The hashmap contains the value if we found it during linear probing
            return probedIndex.IsSome;
        }
        
        /// <summary>
        /// Looks for a bucket index for an existing item
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="hashCode"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private readonly ValueTypeOption<int> LinearProbeForExistingEntry(in DynamicBuffer<Entry<V>> bucket, int hashCode, int startingIndex) {
            for (int i = 0; i < MAX_COUNT; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                int checkIndex = (startingIndex + i) & ~MAX_COUNT;
                
                Entry<V> entry = bucket[checkIndex];
                if (entry.HasValue && entry.HashCode == hashCode) {
                    return ValueTypeOption<int>.Some(checkIndex);
                }
            }
            
            // We've checked all entries. We can't find a suitable slot.
            return ValueTypeOption<int>.None;
        }
        
        // This is taken from https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/
        private static int FibonacciHash(int hash) {
            // This is 2^64 / 1.6180339 (Fibonacci constant)
            const ulong magicNumber = 11400714819323198485;
            
            // We shift 58 bits here as we only need 6 bits (0-63)
            // Note that the bucket count is 64
            return (int)(((ulong)hash * magicNumber) >> 58);
        }
        
        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear(ref DynamicBuffer<Entry<V>> bucket) {
            // Note here that we're not clearing the bucket entries
            // We're only setting each slot to nothing
            for (int i = 0; i < bucket.Length; ++i) {
                bucket[i] = Entry<V>.Nothing;
            }

            this.count = 0;
        }

        public static void ResetValues(ref DynamicBuffer<Entry<V>> bucket) {
            // We are only resetting the values here
            // Count remains the same
            for (int i = 0; i < bucket.Length; ++i) {
                Entry<V> entry = bucket[i];
                if (!entry.HasValue) {
                    // No value
                    continue;
                }
                
                // Reset value
                bucket[i] = Entry<V>.Something(entry.HashCode, default);
            }
        }

        /// <summary>
        /// Holds the values in a DynamicBuffer
        /// This is implemented like a Maybe. A hashcode of zero is considered as a nothing.
        /// </summary>
        [InternalBufferCapacity(64)]
        public readonly struct Entry<T> : IBufferElementData where T : unmanaged, IEquatable<T> {
            private readonly T value;
            private readonly int hashCode;

            public static Entry<T> Nothing {
                get {
                    return new Entry<T>();
                }
            }

            public static Entry<T> Something(int hashCode, in T value) {
                // Hashcode can't be zero because it denotes a nothing value
                DotsAssert.IsTrue(hashCode != 0);
                return new Entry<T>(hashCode, value);
            }

            public Entry(int hashCode, T value) {
                this.value = value;
                this.hashCode = hashCode;
            }

            public T Value {
                get {
                    if (!this.HasValue) {
                        throw new Exception("Trying to access the value when there is none.");
                    }

                    return this.value;
                }
            }

            public int HashCode {
                get {
                    if (!this.HasValue) {
                        throw new Exception("Trying to access hash code when there is none.");
                    }

                    return this.hashCode;
                }
            }

            public bool HasValue {
                get {
                    return this.hashCode != 0;
                }
            }
        }
    }
}