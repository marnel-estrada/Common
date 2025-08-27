using System;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is equivalent to DynamicBufferHashMap<ConditionHashId, bool>. We made it into a concrete type since
    /// there's an issue in Unity 6.2 where TypeManager.GetTypeIndex(Type) and TypeManager.GetTypeIndex<T>()
    /// return different results.
    /// </summary>
    public struct ConditionValueMap : IComponentData {
        private int count;

        public const int MAX_COUNT = 64;
        private const int MAX_COUNT_MINUS_ONE = MAX_COUNT - 1;
        
        /// <summary>
        /// This should be called prior to usage so that all of the slots are prepared.
        /// </summary>
        /// <param name="bucket"></param>
        public static void Init(ref DynamicBuffer<Entry> bucket) {
            bucket.EnsureCapacity(MAX_COUNT);
            for (int i = 0; i < MAX_COUNT; ++i) {
                bucket.Add(Entry.Nothing);
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
        public int AddOrSet(ref DynamicBuffer<Entry> bucket, in ConditionHashId key, in bool value) {
            int hashCode = key.GetHashCode();
            
            // This is the same as hashCode % MAX_COUNT
            int bucketIndex = hashCode & MAX_COUNT_MINUS_ONE;

            if (!bucket[bucketIndex].HasValue) {
                // This means that it's an empty slot. We can place the value here.
                bucket[bucketIndex] = Entry.Something(hashCode, value);
                ++this.count;
                return bucketIndex;
            }
            
            // At this point, there's an existing value at the slot
            // Let's check if they have the same hashcode
            if (bucket[bucketIndex].HashCode == hashCode) {
                // It's the same item. Let's replace the value.
                bucket[bucketIndex] = Entry.Something(hashCode, value);
                return bucketIndex;
            }
            
            // At this point, the item at the resolved index is a different item
            // Let's do linear probing
            // Note here that we start on the next index because bucketIndex is already checked
            int probedIndex = LinearProbeForAdding(ref bucket, hashCode, bucketIndex + 1);
            
            // Note here that we don't add to count if it was a replacement
            this.count += bucket[probedIndex].HasValue ? 0 : 1;
            bucket[probedIndex] = Entry.Something(hashCode, value);

            return probedIndex;
        }
        
        /// <summary>
        /// Look for a bucket index that's empty or of the same hashcode
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="hashCode"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private static int LinearProbeForAdding(ref DynamicBuffer<Entry> bucket, int hashCode, int startingIndex) {
            for (int i = 0; i < MAX_COUNT; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                // Using bitwise operator is faster instead of using modulo
                int checkIndex = (startingIndex + i) & MAX_COUNT_MINUS_ONE;
            
                Entry entry = bucket[checkIndex];
                if (!entry.HasValue || entry.HashCode == hashCode) {
                    return checkIndex;
                }
            }
        
            // We've checked all entries. We can't find a suitable slot.
            throw new Exception("Bucket is full. Can't find an empty or an existing slot with the same hash code.");
        }
        
        public void Remove(ref DynamicBuffer<Entry> bucket, in ConditionHashId key) {
            int hashCode = key.GetHashCode();
            
            // This is the same as hashCode % MAX_COUNT
            int bucketIndex = hashCode & MAX_COUNT_MINUS_ONE;
            
            // Check if the slot at the resolved index is already the item
            Entry entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // This is the item. We remove it.
                bucket[bucketIndex] = Entry.Nothing;
                --this.count;
                return;
            }
            
            // At this point, the entry at the resolved index is a different item
            // Let's linear probe for the item
            int probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);
            if (probedIndex >= 0) {
                // We found the item as the found index is not negative
                bucket[probedIndex] = Entry.Nothing;
                --this.count;
            }
            
            // When probedIndex is None, it just means that the item is not in the HashMap
        }
        
        public readonly ValueTypeOption<bool> Find(in DynamicBuffer<Entry> bucket, in ConditionHashId key) {
            int hashCode = key.GetHashCode();
            
            // This is the same as hashCode % MAX_COUNT
            int bucketIndex = hashCode & MAX_COUNT_MINUS_ONE;
            
            // Check if key is already at the resolved index
            Entry entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // Found the item
                return ValueTypeOption<bool>.Some(entry.Value);
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            int probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);
            if (probedIndex >= 0) {
                return ValueTypeOption<bool>.Some(bucket[probedIndex].Value);
            }
            
            // At this point, we didn't find the item
            return ValueTypeOption<bool>.None;
        }
        
        public bool Contains(in DynamicBuffer<Entry> bucket, in ConditionHashId key) {
            int hashCode = key.GetHashCode();
            
            // This is the same as hashCode % MAX_COUNT
            int bucketIndex = hashCode & MAX_COUNT_MINUS_ONE;
            
            // Check if key is already at the resolved index
            Entry entry = bucket[bucketIndex];
            if (entry.HasValue && entry.HashCode == hashCode) {
                // Found the item
                return true;
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            int probedIndex = LinearProbeForExistingEntry(bucket, hashCode, bucketIndex + 1);

            // The hashmap contains the value if it's not negative
            return probedIndex >= 0;
        }
        
        /// <summary>
        /// Looks for a bucket index for an existing item
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="hashCode"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private static int LinearProbeForExistingEntry(in DynamicBuffer<Entry> bucket, int hashCode, int startingIndex) {
            for (int i = 0; i < MAX_COUNT; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                int checkIndex = (startingIndex + i) & MAX_COUNT_MINUS_ONE;
                
                Entry entry = bucket[checkIndex];
                if (entry.HasValue && entry.HashCode == hashCode) {
                    return checkIndex;
                }
            }
            
            // We've checked all entries. We can't find a suitable slot.
            // We return a negative index to denote that we didn't find a suitable slot.
            return -1;
        }
        
        public int Count => this.count;

        public void Clear(ref DynamicBuffer<Entry> bucket) {
            // Note here that we're not clearing the bucket entries
            // We're only setting each slot to nothing
            for (int i = 0; i < bucket.Length; ++i) {
                bucket[i] = Entry.Nothing;
            }

            this.count = 0;
        }

        public static void ResetValues(ref DynamicBuffer<Entry> bucket) {
            // We are only resetting the values here
            // Count remains the same
            for (int i = 0; i < bucket.Length; ++i) {
                Entry entry = bucket[i];
                if (!entry.HasValue) {
                    // No value
                    continue;
                }
                
                // Reset value
                bucket[i] = Entry.Something(entry.HashCode, false);
            }
        }
        
        /// <summary>
        /// Holds the values in a DynamicBuffer. This acts as the bucket to the hashmap.
        /// This is implemented like a Maybe. A hashcode of zero is considered as a nothing.
        /// </summary>
        [InternalBufferCapacity(64)]
        public readonly struct Entry : IBufferElementData {
            private readonly int hashCode;
            private readonly bool value;

            public static Entry Nothing => new();

            public static Entry Something(int hashCode, in bool value) {
                // Hashcode can't be zero because it denotes a nothing value
                DotsAssert.IsTrue(hashCode != 0);
                return new Entry(hashCode, value);
            }

            private Entry(int hashCode, bool value) {
                this.value = value;
                this.hashCode = hashCode;
            }

            public bool Value {
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

            public bool HasValue => this.hashCode != 0;

            public Entry WithValue(in bool newValue) {
                return Something(this.hashCode, newValue); 
            }
        }
    }
}