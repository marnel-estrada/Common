using CommonEcs;
using Unity.Collections;
using Unity.Entities;

namespace Common {
    /// <summary>
    /// The sweep and prune algorithm is housed here so we don't pass around individual
    /// data containers when we schedule jobs.
    /// </summary>
    public struct SweepPruneContainer {
        private NativeHashMap<Entity, SweepPruneItem> entryMap;

        // We add all endpoints here. Each entry occupies two slots, the min and max.
        // The list does not resize when an entry is removed. The slot position would be reused.
        // We don't sort this. We use another list for sorting.
        private NativeList<EndPoint> endPoints;

        // The integer content here are indices that points to an entry in endPoints
        private NativeList<int> sortedEndPoints;

        // We store inactive positions here so we can reuse them
        private NativeStack<int> inactivePositions;

        public SweepPruneContainer(int initialCapacity, Allocator allocator) {
            this.entryMap = new NativeHashMap<Entity, SweepPruneItem>(initialCapacity, allocator);
            this.endPoints = new NativeList<EndPoint>(initialCapacity * 2, allocator);
            this.sortedEndPoints = new NativeList<int>(initialCapacity * 2, allocator);
            this.inactivePositions = new NativeStack<int>(initialCapacity, allocator);
        }

        public void Dispose() {
            this.entryMap.Dispose();
            this.endPoints.Dispose();
            this.sortedEndPoints.Dispose();
            this.inactivePositions.Dispose();
        }

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Add(Entity entity, float min, float max) {
            
        }

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(Entity entity) {
            
        }

        /// <summary>
        /// Updates the extents of the item
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Update(Entity entity, float min, float max) {
            
        }

        /// <summary>
        /// Sorts the end points
        /// </summary>
        public void Sort() {
            
        }

        /// <summary>
        /// Queries for items that contains the specified linear position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="resultList"></param>
        public void Contains(float position, ref NativeList<Entity> resultList) {
            
        }

        /// <summary>
        /// Queries for items that overlaps the specified min and max extents
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="resultList"></param>
        public void Overlaps(float min, float max, ref NativeList<Entity> resultList) {
            
        }
        
        private struct SweepPruneItem {
            public readonly Entity entity;
            public readonly int listPosition;

            public SweepPruneItem(Entity entity, int listPosition) {
                this.entity = entity;
                this.listPosition = listPosition;
            }
        }

        private struct EndPoint {
            public readonly Entity entity;
            public float position; // Not readonly since we update this every frame
            
            // If it's not minimum, then it can only be maximum
            public readonly bool isMin;

            // We set this to false when an entity is removed
            public bool active;

            public EndPoint(Entity entity, float position, bool isMin) : this() {
                this.entity = entity;
                this.position = position;
                this.isMin = isMin;
            }
        }
    }
}