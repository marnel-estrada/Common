namespace CommonEcs {
    public struct SortedSpriteEntry {
        public int index; // The index of the sprite to its manager
        public int layerOrder;
        public float renderOrder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderOrder"></param>
        public SortedSpriteEntry(int index, int layerOrder, float renderOrder) {
            this.index = index;
            this.layerOrder = layerOrder;
            this.renderOrder = renderOrder;
        }
    }
}
