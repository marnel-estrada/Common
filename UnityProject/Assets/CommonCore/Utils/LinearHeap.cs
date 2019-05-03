using System;

namespace Common {
    /// <summary>
    /// A heap that is internally implemented as a list
    /// </summary>
    public class LinearHeap<T> {
        private readonly SimpleList<T> list;

        private readonly Comparison<T> comparison;

        public LinearHeap(int initialCapacity, Comparison<T> comparison) {
            this.list = new SimpleList<T>(initialCapacity);
            this.comparison = comparison;
        }

        /// <summary>
        /// Adds an item into the heap
        /// The item will be added into its proper place in the heap
        /// </summary>
        /// <param name="newItem"></param>
        public void Add(T newItem) {
            this.list.Add(newItem);

            if (this.list.Count <= 1) {
                // No need to bubble down if there's one or zero items
                return;
            }

            int lastIndex = this.list.Count - 1;
            int correctIndex = lastIndex - 1;

            // Bubble down the item
            while (correctIndex >= 0 && this.comparison(newItem, this.list[correctIndex]) < 0) {
                // At this point, it means that the newItem is lesser than the current item
                // We bubble (swap) the item down the list until we find it's correct position
                this.list.Swap(correctIndex, lastIndex);

                --correctIndex;
                --lastIndex;
            }
        }

        public T Top {
            get {
                Assertion.Assert(this.list.Count > 0);

                // Top is the last item
                return this.list[this.list.Count - 1];
            }
        }

        public T Pop() {
            T top = this.Top;
            this.list.RemoveAt(this.list.Count - 1);

            return top;
        }

        public void Remove(T item) {
            this.list.Remove(item);
        }

        /// <summary>
        /// Fixes the heap. This is used for cases when the values are updated outside of the heap
        /// and we want to ensure that the heap is still consistent
        /// </summary>
        public void Fix() {
            this.list.InsertionSort(this.comparison);
        }

        public void Clear() {
            this.list.Clear();
        }

        public int Count {
            get {
                return this.list.Count;
            }
        }

        public T this[int index] {
            get {
                return this.list[index];
            }
        }
    }
}