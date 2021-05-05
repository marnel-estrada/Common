namespace Common {
    /// <summary>
    /// The idea for this data structure was derived from here http://seanmiddleditch.com/data-structures-for-game-developers-the-slot-map/
    /// It's a dictionary where the keys are integers. Items are kept in a fixed array so access is fast.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuickMap<T> {

        private readonly SimpleList<T> items;
        private readonly SimpleList<int> freeIds;

        /// <summary>
        /// Constructor
        /// </summary>
        public QuickMap() {
            this.items = new SimpleList<T>();
            this.freeIds = new SimpleList<int>();
        }

        /// <summary>
        /// Constructor with buffer count
        /// </summary>
        /// <param name="bufferCount"></param>
        public QuickMap(int bufferCount) {
            this.items = new SimpleList<T>(bufferCount);
            this.freeIds = new SimpleList<int>(bufferCount);
        }

        /// <summary>
        /// Adds an item. Returns its associated int key.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Add(T item) {
            if(this.freeIds.Count > 0) {
                // Use a free slot
                int freeId = this.freeIds[this.freeIds.Count - 1]; // Get the last one
                this.freeIds.RemoveAt(this.freeIds.Count - 1);

                this.items[freeId] = item;

                return freeId;
            }

            // No free IDs
            // Use the end of the list
            this.items.Add(item);
            return this.items.Count - 1;
        }

        /// <summary>
        /// Returns the item with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get(int key) {
            return this.items[key];
        }

        /// <summary>
        /// Removes the item with the specified key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(int key) {
            this.items[key] = default(T);
            this.freeIds.Add(key);
        }

        public int Count {
            get {
                return this.items.Count;
            }
        }

    }
}
