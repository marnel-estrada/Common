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
        private NativeHashMap<Entity, SweepPruneItem> itemMap;

        // Maintains items in a list so we can index them.
        // The entries in sortedIndices then are just integers that indexes here
        private NativeList<SweepPruneItem> masterList;

        private NativeList<int> sortedIndices;

        // We store inactive positions here so we can reuse them
        private NativeStack<int> inactiveMasterIndices;

        public SweepPruneContainer(int initialCapacity, Allocator allocator) {
            this.itemMap = new NativeHashMap<Entity, SweepPruneItem>(initialCapacity, allocator);
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
            }
            else {
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
            
        }

        /// <summary>
        /// Queries for items that overlaps the specified box
        /// </summary>
        /// <param name="box"></param>
        /// <param name="resultList"></param>
        public void Overlaps(Aabb2 box, ref NativeList<Entity> resultList) {
            
        }
        
        private struct SweepPruneItem {
            public Aabb2 box;
            public readonly Entity entity;
            public readonly int masterListIndex;

            public SweepPruneItem(Entity entity, Aabb2 box, int masterListIndex) {
                this.entity = entity;
                this.box = box;
                this.masterListIndex = masterListIndex;
            }

            public bool IsNone => this.entity == Entity.Null;

            // Needs the masterListIndex so it retains its position
            public static SweepPruneItem NoneItem(int masterListIndex) {
                return new SweepPruneItem(Entity.Null, Aabb2.EmptyBounds(), masterListIndex);
            }
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