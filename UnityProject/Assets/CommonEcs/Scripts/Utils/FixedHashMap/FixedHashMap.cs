using System;

using Common;

using Unity.Collections;

namespace CommonEcs {
    public struct FixedHashMap<K, V> 
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        private FixedHashMapBuckets<V> buckets;
        private int count;
        
        public void AddOrSet(K key, V value) {
            int hashCode = key.GetHashCode();
            int bucketIndex = FibonacciHash(hashCode);
            FixedList512<FixedHashMapEntry<V>> bucket = this.buckets[bucketIndex];
            
            // Search for similar key. Replace the value if we find an entry with similar key.
            for (int i = 0; i < bucket.Length; ++i) {
                if (bucket[i].hashCode == hashCode) {
                    // Found an entry with the same hash code
                    // We replace the value
                    bucket[i] = new FixedHashMapEntry<V>(hashCode, value);
                    this.buckets[bucketIndex] = bucket; // Don't forget the update the bucket
                    return;
                }
            }
            
            // At this point, we don't find an entry in the bucket with the same hash code
            // We add an entry
            bucket.Add(new FixedHashMapEntry<V>(hashCode, value));
            ++this.count;
            
            // Update the bucket
            this.buckets[bucketIndex] = bucket;
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
            for (int i = 0; i < FixedHashMapBuckets<V>.Length; ++i) {
                this.buckets[i].Clear();
            }
            
            this.count = 0;
        }
    }
}