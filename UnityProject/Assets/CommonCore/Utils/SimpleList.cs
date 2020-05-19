using UnityEngine;
using System.Collections.Generic;
using System;

// This improved version of the System.Collections.Generic.List that doesn't release the buffer on Clear(), resulting in better performance and less garbage collection.
namespace Common {
    public class SimpleList<T> {
        /// <summary>
        /// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use SimpleList.size.
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
        /// </summary>
        private int size;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleList() {
        }

        /// <summary>
        /// Constructor with specified buffer size
        /// </summary>
        /// <param name="bufferSize"></param>
        public SimpleList(int bufferSize) {
            this.buffer = new T[bufferSize];
        }

        /// <summary>
        /// For 'foreach' functionality.
        /// </summary>
        public IEnumerator<T> GetEnumerator() {
            if (this.buffer != null) {
                for (int i = 0; i < this.size; ++i) {
                    yield return this.buffer[i];
                }
            }
        }

        /// <summary>
        /// Convenience function. I recommend using .buffer instead.
        /// </summary>
        public T this[int i] {
            get {
                return this.buffer[i];
            }

            set {
                this.buffer[i] = value;
            }
        }

        private const int INITIAL_BUFFER_SIZE = 4;

        /// <summary>
        /// Helper function that expands the size of the array, maintaining the content.
        /// </summary>
        void AllocateMore() {
            T[] newList = (this.buffer != null) ? new T[Mathf.Max(this.buffer.Length << 1, INITIAL_BUFFER_SIZE)] : new T[INITIAL_BUFFER_SIZE];
            if (this.buffer != null && this.size > 0) this.buffer.CopyTo(newList, 0);
            this.buffer = newList;
        }

        /// <summary>
        /// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
        /// Call this function only if you are sure that the buffer won't need to resize anytime soon.
        /// </summary>
        void Trim() {
            if (this.size > 0) {
                if (this.size < this.buffer.Length) {
                    T[] newList = new T[this.size];
                    for (int i = 0; i < this.size; ++i) newList[i] = this.buffer[i];
                    this.buffer = newList;
                }
            } else
                this.buffer = null;
        }

        /// <summary>
        /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
        /// </summary>
        public void Clear() {
            this.size = 0;
        }

        /// <summary>
        /// Clear the array and release the used memory.
        /// </summary>
        public void Release() {
            this.size = 0;
            this.buffer = null;
        }

        /// <summary>
        /// Add the specified item to the end of the list.
        /// </summary>
        public void Add(T item) {
            if (this.buffer == null || this.size == this.buffer.Length) AllocateMore();
            this.buffer[this.size++] = item;
        }

        /// <summary>
        /// Insert an item at the specified index, pushing the entries back.
        /// </summary>
        public void Insert(int index, T item) {
            Assertion.IsTrue(0 <= index && index < this.size, "Invalid index");

            if (this.buffer == null || this.size == this.buffer.Length) AllocateMore();

            if (index < this.size) {
                for (int i = this.size; i > index; --i) this.buffer[i] = this.buffer[i - 1];
                this.buffer[index] = item;
                ++this.size;
            } else Add(item);
        }

        /// <summary>
        /// Returns 'true' if the specified item is within the list.
        /// </summary>
        public bool Contains(T item) {
            // Has the item if its index has been found
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
        /// </summary>
        public bool Remove(T item) {
            if (this.buffer != null) {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;

                for (int i = 0; i < this.size; ++i) {
                    if (comp.Equals(this.buffer[i], item)) {
                        --this.size;
                        this.buffer[i] = default(T);
                        for (int b = i; b < this.size; ++b) this.buffer[b] = this.buffer[b + 1];
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Remove an item at the specified index.
        /// </summary>
        public void RemoveAt(int index) {
            Assertion.IsTrue(0 <= index && index < this.size, "Invalid index");

            if (this.buffer != null && index < this.size) {
                --this.size;
                this.buffer[index] = default(T);
                for (int b = index; b < this.size; ++b) this.buffer[b] = this.buffer[b + 1];
            }
        }

        /// <summary>
        /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
        /// </summary>
        public T[] ToArray() {
            Trim();
            return this.buffer;
        }

        /// <summary>
        /// List.Sort equivalent.
        /// </summary>
        public void Sort(Comparison<T> comparer) {
            bool changed = true;

            while (changed) {
                changed = false;

                for (int i = 1; i < this.size; ++i) {
                    if (comparer.Invoke(this.buffer[i - 1], this.buffer[i]) > 0) {
                        T temp = this.buffer[i];
                        this.buffer[i] = this.buffer[i - 1];
                        this.buffer[i - 1] = temp;
                        changed = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sort using insertion sort (based from https://en.wikipedia.org/wiki/Insertion_sort)
        /// </summary>
        /// <param name="comparer"></param>
        public void InsertionSort(Comparison<T> comparer) {
            int count = this.Count;
            for (int i = 1; i < count; ++i) {
                T x = this.buffer[i];
                int j = i - 1;

                while (j >= 0 && comparer(this.buffer[j], x) > 0) {
                    this.buffer[j + 1] = this.buffer[j];
                    --j;
                }

                this.buffer[j + 1] = x;
            }
        }

        /// <summary>
        /// Swaps the contents in the specified indeces
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Swap(int a, int b) {
            T temp = this.buffer[a];
            this.buffer[a] = this.buffer[b];
            this.buffer[b] = temp;
        }

        public int Count {
            get {
                return this.size;
            }
        }

        /// <summary>
        /// Can be used for faster access (prevent method overhead)
        /// </summary>
        public T[] Buffer {
            get {
                return this.buffer;
            }
        }

        /**
		 * Returns whether or not the list is empty.
		 */
        public bool IsEmpty() {
            return this.size <= 0;
        }

        public delegate void Visitor(T item);

        /**
         * Traverse the items in the list using a visitor.
         */
        public void TraverseItems(Visitor visitor) {
            int count = this.Count;
            for (int i = 0; i < count; ++i) {
                visitor(this[i]);
            }
        }

        /// <summary>
        /// Returns the index of the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item) {
            if (this.buffer == null) {
                return -1;
            }

            for (int i = 0; i < this.size; ++i) {
                if (this.buffer[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        public ReadOnlySimpleList<T> AsReadOnly {
            get {
                // Note this is not garbage as ReadOnlySimpleList is a struct
                return new ReadOnlySimpleList<T>(this);
            }
        }
    }
}