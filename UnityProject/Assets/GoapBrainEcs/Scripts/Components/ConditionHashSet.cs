using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// We implement this as IComponentData so we don't have to wrap it inside an IComponentData
    /// </summary>
    public struct ConditionHashSet : IComponentData {
        private int count;
        private ConditionBuckets16 buckets;
        
        /// <summary>
        /// Adds an item
        /// </summary>
        /// <param name="item"></param>
        public void Add(Condition item) {
            int bucketIndex = item.GetHashCode() % ConditionBuckets16.Length;
            if (Contains(item, bucketIndex)) {
                // Already contains the item. No need to add.
                return;
            }
            
            this.buckets[bucketIndex].Add(item);
            ++this.count;
        }

        public void Add(ushort conditionId, bool value) {
            Add(new Condition(conditionId, value));
        }

        /// <summary>
        /// Returns whether or not the HashSet contains the specified Condition item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Condition item) {
            int bucketIndex = item.GetHashCode() % ConditionBuckets16.Length;
            return Contains(item, bucketIndex);
        }

        private bool Contains(Condition item, int bucketIndex) {
            ConditionList16 entryList = this.buckets[bucketIndex];
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
        public void Remove(Condition item) {
            int bucketIndex = item.GetHashCode() % ConditionBuckets16.Length;
            ConditionList16 entryList = this.buckets[bucketIndex];
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

        public void Remove(ushort conditionId, bool value) {
            Remove(new Condition(conditionId, value));
        }

        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear() {
            for (int i = 0; i < ConditionBuckets16.Length; ++i) {
                this.buckets[i].Clear();
            }
            
            this.count = 0;
        }
        
        // We will only implement enumeration when it's needed. It's not needed for now.
    }
}