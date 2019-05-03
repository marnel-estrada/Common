namespace CommonEcs {
    /// <summary>
    /// A stack allocated HashSet of ushorts that can be placed inside an IComponentData struct
    /// This HashSet only allows up to 256 of entries (16 buckets with 16 entries each)
    /// </summary>
    public struct UShortHashSet {
        private int count;
        private UShortBuckets16 buckets;

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <param name="item"></param>
        public void Add(ushort item) {
            int bucketIndex = item % UShortBuckets16.Length;
            if (Contains(item, bucketIndex)) {
                // Already contains the item. No need to add.
                return;
            }
            
            this.buckets[bucketIndex].Add(item);
            ++this.count;
        }

        /// <summary>
        /// Returns whether or not the HashSet contains the specified ushort item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ushort item) {
            int bucketIndex = item % UShortBuckets16.Length;
            return Contains(item, bucketIndex);
        }

        private bool Contains(ushort item, int bucketIndex) {
            UShortList16 entryList = this.buckets[bucketIndex];
            for (int i = 0; i < entryList.Count; ++i) {
                if (entryList[i] == item) {
                    // Item was found in the entry list
                    return true;
                }
            }

            // No entry found
            return false;
        }

        /// <summary>
        /// Removes the specified item
        /// </summary>
        /// <param name="item"></param>
        public void Remove(ushort item) {
            int bucketIndex = item % UShortBuckets16.Length;
            UShortList16 entryList = this.buckets[bucketIndex];
            for (int i = 0; i < entryList.Count; ++i) {
                if (entryList[i] == item) {
                    // Item found. Remove it.
                    // Note here that remove using buckets because it returns the reference of the list,
                    // not a copy like entryList here
                    this.buckets[bucketIndex].RemoveAt(i);
                    --this.count;
                    return;
                }
            }
        }

        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear() {
            for (int i = 0; i < UShortBuckets16.Length; ++i) {
                this.buckets[i].Clear();
            }
            
            this.count = 0;
        }
        
        // We will only implement enumeration when it's needed. It's not needed for now.
    }
}