using System.Collections.Generic;
using CommonEcs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Common {
    /// <summary>
    /// The sweep and prune algorithm is housed here so we don't pass around individual
    /// data containers when we schedule jobs.
    /// </summary>
    public struct SweepPruneContainer {
        // Made this public so the items can be updated in a parallel job
        public NativeParallelHashMap<Entity, SweepPruneItem> itemMap;

        // Maintains items in a list so we can index them.
        // The entries in sortedIndices then are just integers that indexes here
        // Made this public so the items can be updated in a parallel job
        public NativeList<SweepPruneItem> masterList;

        private NativeList<int> sortedIndices;

        // We store inactive positions here so we can reuse them
        private NativeStack<int> inactiveMasterIndices;

        public SweepPruneContainer(int initialCapacity, Allocator allocator) {
            this.itemMap = new NativeParallelHashMap<Entity, SweepPruneItem>(initialCapacity, allocator);
            this.masterList = new NativeList<SweepPruneItem>(initialCapacity, allocator);
            this.sortedIndices = new NativeList<int>(initialCapacity, allocator);
            this.inactiveMasterIndices = new NativeStack<int>(initialCapacity, allocator);
        }

        public void Dispose() {
            this.itemMap.Dispose();
            this.masterList.Dispose();
            this.sortedIndices.Dispose();
            this.inactiveMasterIndices.Dispose();
        }

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="box"></param>
        public void Add(Entity entity, Aabb2 box) {
            if (this.inactiveMasterIndices.Count > 0) {
                // There are inactive master indices. We reuse one.
                int masterIndex = this.inactiveMasterIndices.Pop();
                SweepPruneItem item = new(entity, box, masterIndex);
                this.itemMap[entity] = item;
                this.masterList[masterIndex] = item;
            } else {
                // There are no inactive master indices. We add to the masterList.
                int masterIndex = this.masterList.Length;
                SweepPruneItem item = new(entity, box, masterIndex);
                this.itemMap[entity] = item;
                this.masterList.Add(item);
                
                // We also add to the sorted list since its a new entry
                this.sortedIndices.Add(masterIndex);
            }
        }

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(Entity entity) {
            if (!this.itemMap.TryGetValue(entity, out SweepPruneItem item)) {
                // Entity was not added in sweep and prune
                return;
            }

            // Note here that we don't remove entries from the masterList
            // We are just setting them to none
            this.masterList[item.masterListIndex] = SweepPruneItem.NoneItem(item.masterListIndex);
            this.inactiveMasterIndices.Push(item.masterListIndex);
            
            this.itemMap.Remove(entity);
        }

        /// <summary>
        /// Updates the extents of the item. Note here that the box is assumed to be in world space.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="box"></param>
        public void Update(Entity entity, Aabb2 box) {
            SweepPruneItem item = this.itemMap[entity];
            item.box = box;
            this.itemMap[entity] = item; // Modify the one in the map
            this.masterList[item.masterListIndex] = item; // Modify the one in master list
        }

        /// <summary>
        /// Sorts the end points
        /// </summary>
        public void Sort() {
            SweepPruneComparer comparer = new(this.masterList);
            NativeContainerUtils.InsertionSort(ref this.sortedIndices, comparer);
        }

        /// <summary>
        /// Queries for items that contains the specified linear position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="resultList"></param>
        public void Contains(float2 position, ref NativeList<Entity> resultList) {
            resultList.Clear();
            
            ValueTypeOption<int> startingIndex = ResolveStartingIndex(position);
            if (startingIndex.IsNone) {
                // No entry contains the position
                return;
            }

            // Check for each box from the starting index until we find the disjoint
            int start = startingIndex.ValueOrError();
            for (int i = start; i < this.sortedIndices.Length; i++) {
                int masterIndex = this.sortedIndices[i];
                SweepPruneItem item = this.masterList[masterIndex];
                if (position.x < item.box.Min.x) {
                    // Found the disjoint
                    return;
                }

                if (item.box.Contains(position)) {
                    // Found an item that contains the position
                    resultList.Add(item.entity);
                }
            }
        }
        
        private ValueTypeOption<int> ResolveStartingIndex(float2 position) {
            int left = 0;
            int right = this.sortedIndices.Length - 1;

            while (left <= right) {
                int mid = left + (right - left) / 2;
                int midMasterIndex = this.sortedIndices[mid];

                Aabb2 midBox = this.masterList[midMasterIndex].box;
                if (midBox.Max.x < position.x) {
                    // Disjoint at right side of the item. Move to right.
                    left = mid + 1;
                    continue;
                }

                if (midBox.Min.x > position.x) {
                    // Disjoint at left side of the item. Move to left.
                    right = mid - 1;
                    continue;
                }
                
                // At this point, midItem overlaps with the position
                // But we check if the previous items still overlaps. If it does, we continue moving left.
                int previous = mid - 1;
                if (previous < 0) {
                    // We are already at the first of the array. We found our answer
                    return ValueTypeOption<int>.Some(mid);
                }

                int previousMasterIndex = this.sortedIndices[previous];
                Aabb2 previousBox = this.masterList[previousMasterIndex].box;
                if (previousBox.Max.x < position.x) {
                    // Disjoint at previous item. The current mid item is already the starting point.
                    return ValueTypeOption<int>.Some(mid);
                }
                
                // This means that previous item still overlaps with query box. We haven't found
                // the starting index yet.
                // Let's continue moving to the left
                right = mid - 1;
            }
            
            // No entry overlaps with the specified position
            return ValueTypeOption<int>.None;
        }

        /// <summary>
        /// Queries for items that overlaps the specified box
        /// </summary>
        /// <param name="box"></param>
        /// <param name="resultList"></param>
        public void Overlaps(Aabb2 box, ref NativeList<Entity> resultList) {
            // TODO Implement
        }

        // Note here that we are only comparing indices to the masterList
        private readonly struct SweepPruneComparer : IComparer<int> {
            private readonly NativeList<SweepPruneItem> masterList;

            public SweepPruneComparer(NativeList<SweepPruneItem> masterList) {
                this.masterList = masterList;
            }
            
            public int Compare(int x, int y) {
                // We compare by min X
                SweepPruneItem xItem = this.masterList[x];
                SweepPruneItem yItem = this.masterList[y];

                if (xItem.box.Min.x < yItem.box.Min.x) {
                    return -1;
                }
                
                if (xItem.box.Min.x > yItem.box.Min.x) {
                    return 1;
                }
                
                // Equal
                return 0;
            }
        }
    }
}