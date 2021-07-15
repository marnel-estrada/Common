using System;

namespace CommonEcs {
    public struct LinearHashMap<K, V> 
        where K : unmanaged, IEquatable<K>
        where V : unmanaged, IEquatable<V> {
        private LinearHashMapBucket<K, V> bucket;
        private int count;
        
        public void AddOrSet(in K key, in V value) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);

            if (!this.bucket[bucketIndex].hasValue) {
                // This means that it's an empty slot. We can place the value here.
                this.bucket[bucketIndex] = new LinearHashMapEntry<K, V>(key, hashCode, value);
                ++this.count;
            }
            
            // At this point, there's an existing value at the slot
            // Let's check if they have the same hashcode
            if (this.bucket[bucketIndex].hashCode == hashCode) {
                // It's the same item. We can replace the value.
                // Note here that we don't update the count since we're just replacing a value.
                this.bucket[bucketIndex] = new LinearHashMapEntry<K, V>(key, hashCode, value);
                return;
            }
            
            // At this point, the item at the resolved index is a different item
            // Let's do linear probing
            // Note here that we start on the next index because bucketIndex is already checked
            int probedIndex = LinearProbeForAdding(hashCode, bucketIndex + 1);
            
            // Note here that we don't add to count if it was a replacement
            this.count += this.bucket[probedIndex].hasValue ? 0 : 1;
            this.bucket[probedIndex] = new LinearHashMapEntry<K, V>(key, hashCode, value);
        }

        private readonly int LinearProbeForAdding(int hashCode, int startingIndex) {
            const int maxCount = LinearHashMapBucket<K, V>.LENGTH;
            const int maxCountMinusOne = maxCount - 1;
            
            for (int i = 0; i < maxCount; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                // Using bitwise operator is faster instead of using modulo
                int checkIndex = (startingIndex + i) & maxCountMinusOne;
                LinearHashMapEntry<K, V> entry = this.bucket[checkIndex];
                if (!entry.hasValue || entry.hashCode == hashCode) {
                    return checkIndex;
                }
            }
            
            // We've checked all entries. We can't find a suitable slot.
            throw new Exception("Bucket is full. Can't find an empty or an existing slot with the same hash code.");
        }
        
        public void Remove(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check if the slot at the resolved index is already the item
            LinearHashMapEntry<K, V> entry = this.bucket[bucketIndex];
            if (entry.hasValue && entry.hashCode == hashCode) {
                // This is the item. We remove it.
                this.bucket[bucketIndex] = LinearHashMapEntry<K, V>.Nothing;
                --this.count;
                return;
            }
            
            // At this point, the entry at the resolved index is a different item
            // Let's linear probe for the item
            int probedIndex = LinearProbeForExistingEntry(hashCode, bucketIndex + 1);
            if (probedIndex >= 0) {
                // We found the item as the found index is not negative
                this.bucket[probedIndex] = LinearHashMapEntry<K, V>.Nothing;
                --this.count;
            }
            
            // When probedIndex is None, it just means that the item is not in the HashMap
        }
        
        /// <summary>
        /// Looks for a bucket index for an existing item
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="hashCode"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private readonly int LinearProbeForExistingEntry(int hashCode, int startingIndex) {
            const int maxCount = LinearHashMapBucket<K, V>.LENGTH;
            const int maxCountMinusOne = maxCount - 1;
            
            for (int i = 0; i < maxCount; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                int checkIndex = (startingIndex + i) & maxCountMinusOne;
                
                LinearHashMapEntry<K, V> entry = this.bucket[checkIndex];
                if (entry.hasValue && entry.hashCode == hashCode) {
                    return checkIndex;
                }
            }
            
            // We've checked all entries. We can't find a suitable slot.
            // We return a negative index to denote that we didn't find a suitable slot.
            return -1;
        }
        
        public readonly ValueTypeOption<V> Find(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check if key is already at the resolved index
            LinearHashMapEntry<K, V> entry = this.bucket[bucketIndex];
            if (entry.hasValue && entry.hashCode == hashCode) {
                // Found the item
                return ValueTypeOption<V>.Some(entry.value);
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            int probedIndex = LinearProbeForExistingEntry(hashCode, bucketIndex + 1);
            if (probedIndex >= 0) {
                return ValueTypeOption<V>.Some(this.bucket[probedIndex].value);
            }

            // Not found
            return ValueTypeOption<V>.None;
        }

        public bool ContainsKey(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            
            // Check if key is already at the resolved index
            LinearHashMapEntry<K, V> entry = this.bucket[bucketIndex];
            if (entry.hasValue && entry.hashCode == hashCode) {
                // Found the item
                return true;
            }
            
            // At this point, the item in bucketIndex is a different item.
            // Let's do a linear probe
            int probedIndex = LinearProbeForExistingEntry(hashCode, bucketIndex + 1);
            if (probedIndex >= 0) {
                return true;
            }

            // Not found
            return false;
        }

        // This is taken from https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/
        private static int FibonacciHash(int hash) {
            // This is 2^64 / 1.6180339 (Fibonacci constant)
            const ulong magicNumber = 11400714819323198485;
            
            // We shift 57 bits here as we only need 7 bits (0 - 127)
            // Note that the bucket count is 128
            return (int)(((ulong)hash * magicNumber) >> 57);
        }
        
        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear() {
            for (int i = 0; i < FixedHashMapBuckets<K, V>.Length; ++i) {
                this.bucket[i] = default;
            }
            
            this.count = 0;
        }

        public LinearHashMapBucket<K, V>.Enumerator Entries {
            get {
                return this.bucket.GetEnumerator();
            }
        }
    }
}