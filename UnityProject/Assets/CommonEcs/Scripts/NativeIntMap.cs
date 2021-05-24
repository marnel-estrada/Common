using System;

using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// A container that works like Dictionary<int, T> where the int keys can be reused.
    /// Internally, the items are maintained in a list for faster access instead of an actual
    /// HashMap implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NativeIntMap<T> : IDisposable where T : struct {
        private NativeList<T> itemList;
        private NativeStack<int> unusedKeys;

        public NativeIntMap(Allocator allocator) {
            this.itemList = new NativeList<T>(4, allocator);
            this.unusedKeys = new NativeStack<int>(4, allocator);
        }

        /// <summary>
        /// Used to query the next available index before adding a new item
        /// </summary>
        public int NextAvailableIndex {
            get {
                if (this.unusedKeys.Count > 0) {
                    return this.unusedKeys.Pop();
                }
                
                // There are no more unused keys
                // We use the end of the list
                return this.itemList.Length;
            }
        }

        public void Add(int index, T item) {
            if (index < this.itemList.Length) {
                // This means that the current capacity of the list can still contain the item
                this.itemList[index] = item;
            } else {
                if (index > this.itemList.Length) {
                    throw new Exception($"Invalid index {index}. Should just be the length of the internal list {this.itemList.Length}");
                }
                
                this.itemList.Add(item);

                if (index == this.itemList.Length) {
                    throw new Exception("NativeIntMap has an invalid state");
                }
            }
        }

        /// <summary>
        /// This is different from add such that it doesn't add a new slot at all
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Set(int index, T item) {
            this.itemList[index] = item;
        }

        public void Remove(int index) {
            this.itemList[index] = default;
            this.unusedKeys.Push(index);
        }

        public readonly T this[int index] {
            get {
                return this.itemList[index];
            }
        }

        public readonly bool TryGetValue(int index, out T value) {
            if (0 <= index && index < this.itemList.Length) {
                value = this.itemList[index];

                return true;
            }

            // Invalid index
            value = default;
            return false;
        }

        public void Clear() {
            this.unusedKeys.Clear();
            this.itemList.Clear();
        }

        public void Dispose() {
            if (this.itemList.IsCreated) {
                this.itemList.Dispose();
                this.unusedKeys.Dispose();
            }
        }
    }
}