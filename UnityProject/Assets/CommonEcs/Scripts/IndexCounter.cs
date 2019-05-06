namespace Common.Ecs {
    /// <summary>
    /// A utility struct that keeps track of an index
    /// </summary>
    public struct IndexCounter {

        private uint currentIndex;

        /// <summary>
        /// Returns the next index
        /// </summary>
        /// <returns></returns>
        public uint Next() {
            uint next = this.currentIndex;
            ++this.currentIndex;
            return next;
        }

        /// <summary>
        /// Resets the counter
        /// </summary>
        public void Reset() {
            this.currentIndex = 0;
        }

    }
}
