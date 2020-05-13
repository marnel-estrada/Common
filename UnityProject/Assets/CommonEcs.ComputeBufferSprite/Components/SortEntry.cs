using System;

namespace CommonEcs {
    public struct SortEntry : IComparable<SortEntry> {
        public int index; // The index of the sprite to the master list in its draw instance
        public int layerOrder;
        public float renderOrder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderOrder"></param>
        public SortEntry(int index, int layerOrder, float renderOrder) {
            this.index = index;
            this.layerOrder = layerOrder;
            this.renderOrder = renderOrder;
        }

        public int CompareTo(SortEntry other) {
            if (this.layerOrder < other.layerOrder) {
                return -1;
            }

            if (this.layerOrder > other.layerOrder) {
                return 1;
            }

            // At this point, they have the same layerOrder
            // We check the renderOrder
            if (this.renderOrder < other.renderOrder) {
                return -1;
            }

            if (this.renderOrder > other.renderOrder) {
                return 1;
            }

            // They are equal
            return 0;
        }
    }
}