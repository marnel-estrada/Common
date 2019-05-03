using System.Collections.Generic;

namespace Common {
    /**
     * An abstraction of a list that is immutable. Commonly used for data only instances.
     */
    public class ImmutableList<T> {
        private readonly List<T> list;

        /**
         * Constructor with specified elements.
         */
        public ImmutableList(IEnumerable<T> elements) {
            this.list = new List<T>();

            // copy items
            foreach (T element in elements) {
                this.list.Add(element);
            }
        }

        /**
         * Returns the number of elements.
         */
        public int Count {
            get {
                return this.list.Count;
            }
        }

        /**
         * Returns the element at the specified index.
         */
        public T GetAt(int index) {
            return this.list[index];
        }
    }
}