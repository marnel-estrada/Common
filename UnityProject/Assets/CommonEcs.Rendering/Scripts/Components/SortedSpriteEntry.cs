using System;

namespace CommonEcs {
    public readonly struct SortedSpriteEntry : IComparable<SortedSpriteEntry> {
        public readonly int index; // The index of the sprite to its manager
        public readonly int layerOrder;
        public readonly float renderOrder;
        public readonly float renderOrderDueToPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderOrder"></param>
        public SortedSpriteEntry(int index, int layerOrder, float renderOrder, float renderOrderDueToPosition) {
            this.index = index;
            this.layerOrder = layerOrder;
            this.renderOrder = renderOrder;
            this.renderOrderDueToPosition = renderOrderDueToPosition;
        }

        public int CompareTo(SortedSpriteEntry other) {
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
            
            // At this point, they have the same renderOrder
            // We check renderOrderDueToPosition
            if (this.renderOrderDueToPosition < other.renderOrderDueToPosition) {
                return -1;
            }

            if (this.renderOrderDueToPosition > other.renderOrderDueToPosition) {
                return 1;
            }

            // They are equal
            return 0;
        }
    }
}
