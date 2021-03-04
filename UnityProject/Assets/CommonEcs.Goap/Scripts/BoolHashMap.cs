using System;

namespace CommonEcs.Goap {
    /// <summary>
    /// The generic type T here is the key. The type value is always a boolean.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct BoolHashMap {
        private BoolHashMapBucketArray16 buckets;
        private int count;

        public const int MAX_ITEMS = BoolHashMapBucket.MAX * BoolHashMapBucketArray16.Length; 

        public void AddOrSet(int hashCode, bool value) {
            if (this.count >= MAX_ITEMS) {
                throw new Exception("The maximum number of items has already been reached");
            }
            
            int bucketIndex = FibonacciHash(hashCode);
            BoolHashMapBucket bucket = this.buckets[bucketIndex];
            
            // Search for similar key. Replace the value if we find an entry with similar key.
            for (int i = 0; i < bucket.Count; ++i) {
                if (bucket.GetHashCodeAtIndex(i) == hashCode) {
                    // Found an entry with the same hash code
                    // We replace the value
                    bucket.SetValueAtIndex(i, value);
                    this.buckets[bucketIndex] = bucket; // Don't forget the update the bucket
                    return;
                }
            }
            
            // At this point, we don't find an entry in the bucket with the same hash code
            // We add an entry
            bucket.Add(hashCode, value);
            ++this.count;
            
            // Update the bucket
            this.buckets[bucketIndex] = bucket;
        }

        public void Remove(int hashCode) {
            int bucketIndex = FibonacciHash(hashCode);
            BoolHashMapBucket bucket = this.buckets[bucketIndex];
            if (bucket.Remove(hashCode)) {
                // Decrement the count only when an item was indeed removed from the bucket
                --this.count;
                
                // Don't forget to modify
                this.buckets[bucketIndex] = bucket;
            }
        }

        public ValueTypeOption<bool> Find(int hashCode) {
            int bucketIndex = FibonacciHash(hashCode);
            BoolHashMapBucket bucket = this.buckets[bucketIndex];
            return bucket.GetValue(hashCode);
        }

        public bool Contains(int hashCode) {
            int bucketIndex = FibonacciHash(hashCode);
            BoolHashMapBucket bucket = this.buckets[bucketIndex];
            return bucket.Contains(hashCode);
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
            for (int i = 0; i < BoolHashMapBucketArray16.Length; ++i) {
                this.buckets[i].Clear();
            }

            this.count = 0;
        }
    }
}