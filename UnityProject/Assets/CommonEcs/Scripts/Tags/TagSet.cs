using System;

namespace CommonEcs {
    /// <summary>
    /// Maintains a bucket like structure to store the indices of tags. 
    /// </summary>
    public struct TagSet {
        private IntStackArray32 bucket;
        
        public readonly int id;
        
        // Used 32 here because an integer only has 32 bits
        private const int MAX_COUNT = 32;
        private const int MAX_COUNT_MINUS_ONE = MAX_COUNT - 1;

        public TagSet(int id) : this() {
            this.id = id;
        }

        /// <summary>
        /// Returns the index that can be used for the specified tagHashCode.
        /// </summary>
        /// <param name="tagHashCode"></param>
        /// <returns></returns>
        public int Add(int tagHashCode) {
            int bucketIndex = tagHashCode & MAX_COUNT_MINUS_ONE;

            if (this.bucket[bucketIndex] == 0) {
                // This means that the slot is empty. We can store the hashcode here.
                this.bucket[bucketIndex] = tagHashCode;
                return bucketIndex;
            }
            
            // At this point, there's an existing value at the slot
            // Let's check if they have the same hashcode
            if (this.bucket[bucketIndex] == tagHashCode) {
                // It's the same item. This means that the tag was already added on this set.
                return bucketIndex;
            }
            
            // At this point, the item at the resolved index is a different item
            // Let's do linear probing
            // Note here that we start on the next index because bucketIndex is already checked
            int probedIndex = LinearProbeForEmptyOrExisting(tagHashCode, bucketIndex + 1);
            this.bucket[probedIndex] = tagHashCode;

            return probedIndex;
        }

        private readonly int LinearProbeForEmptyOrExisting(int tagHashCode, int startingIndex) {
            for (int i = 0; i < MAX_COUNT; ++i) {
                // Note here that we go back to zero when we hit the max count
                // In other words, it continues to search from the start of the bucket
                // Using bitwise operator is faster instead of using modulo
                int checkIndex = (startingIndex + i) & MAX_COUNT_MINUS_ONE;

                int currentHashCode = this.bucket[checkIndex];
                if (currentHashCode == 0 || currentHashCode == tagHashCode) {
                    // This means that it's an empty slot or it's a slot with the same hash code
                    // We can use this slot
                    return checkIndex;
                }
            }
        
            // We've checked all entries. We can't find a suitable slot.
            throw new Exception("Bucket is full. Can't find an empty or an existing slot with the same hash code.");
        }

        public readonly int GetIndex(int tagHashCode) {
            int bucketIndex = tagHashCode & MAX_COUNT_MINUS_ONE;

            if (this.bucket[bucketIndex] == 0) {
                // This means that the slot is empty. The tag wasn't added prior to getting its index.
                // ReSharper disable once UseStringInterpolation (due to Burst usage)
                throw new Exception(string.Format("Tag {0} wasn't added prior to calling GetIndex().", tagHashCode));
            }
            
            // At this point, there's an existing value at the slot
            // Let's check if they have the same hashcode
            if (this.bucket[bucketIndex] == tagHashCode) {
                // It's the same item. This means that the tag was already added on this set.
                return bucketIndex;
            }
            
            // At this point, the item at the resolved index is a different item
            // Let's do linear probing
            // Note here that we start on the next index because bucketIndex is already checked
            return LinearProbeForEmptyOrExisting(tagHashCode, bucketIndex + 1);
        }
    }
}