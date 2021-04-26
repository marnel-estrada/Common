using System;

using Unity.Collections;

namespace CommonEcs {
    public struct FixedHashMap<K, V> 
        where K : unmanaged, IEquatable<K>
        where V : unmanaged, IEquatable<V> {
        private FixedHashMapBuckets<K, V> buckets;
        private int count;
        
        public void AddOrSet(in K key, in V value) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            ref FixedList512<FixedHashMapEntry<K, V>> bucket = ref this.buckets[bucketIndex];
            
            // Search for similar key. Replace the value if we find an entry with similar key.
            for (int i = 0; i < bucket.Length; ++i) {
                if (bucket[i].hashCode == hashCode) {
                    // Found an entry with the same hash code
                    // We replace the value
                    bucket[i] = new FixedHashMapEntry<K, V>(key, value);
                    return;
                }
            }
            
            // At this point, we don't find an entry in the bucket with the same hash code
            // We add an entry
            bucket.Add(new FixedHashMapEntry<K, V>(key, value));
            ++this.count;
        }
        
        public void Remove(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            ref FixedList512<FixedHashMapEntry<K, V>> bucket = ref this.buckets[bucketIndex];

            // Search for the key in the value list and remove that
            for (int i = 0; i < bucket.Length; ++i) {
                FixedHashMapEntry<K, V> entry = bucket[i];
                if (entry.hashCode == hashCode) {
                    // Found the item to remove
                    bucket.RemoveAt(i);

                    // Update the count
                    --this.count;

                    break;
                }
            }
        }
        
        public readonly ValueTypeOption<V> Find(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            ref FixedList512<FixedHashMapEntry<K, V>> bucket = ref this.buckets[bucketIndex];

            // Search for the value with the same key
            for (int i = 0; i < bucket.Length; ++i) {
                FixedHashMapEntry<K, V> entry = bucket[i];
                if (entry.hashCode == hashCode) {
                    // We found it
                    return ValueTypeOption<V>.Some(entry.value);
                }
            }

            // Not found
            return ValueTypeOption<V>.None;
        }

        public bool ContainsKey(in K key) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            ref FixedList512<FixedHashMapEntry<K, V>> bucket = ref this.buckets[bucketIndex];

            // Search for the value with the same key
            for (int i = 0; i < bucket.Length; ++i) {
                FixedHashMapEntry<K, V> entry = bucket[i];
                if (entry.hashCode == hashCode) {
                    // Found the item
                    return true;
                }
            }

            return false;
        }

        // This is taken from https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/
        private static int FibonacciHash(int hash) {
            // This is 2^64 / 1.6180339 (Fibonacci constant)
            const ulong magicNumber = 11400714819323198485;
            
            // We shift 60 bits here as we only need 4 bits (0-15)
            // Note that the bucket count is 16
            return (int)(((ulong)hash * magicNumber) >> 60);
        }
        
        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear() {
            for (int i = 0; i < FixedHashMapBuckets<K, V>.Length; ++i) {
                this.buckets[i].Clear();
            }
            
            this.count = 0;
        }

        public FixedHashMapBuckets<K, V>.Enumerator BucketsEnumerator {
            get {
                return this.buckets.GetEnumerator();
            }
        }
    }
}