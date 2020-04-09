namespace CommonEcs {
    public struct SortedEntry {
        public int index; // The index of the sprite to the master list in its draw instance
        public int layerOrder;
        public float renderOrder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index"></param>
        /// <param name="renderOrder"></param>
        public SortedEntry(int index, int layerOrder, float renderOrder) {
            this.index = index;
            this.layerOrder = layerOrder;
            this.renderOrder = renderOrder;
        }
    }
}