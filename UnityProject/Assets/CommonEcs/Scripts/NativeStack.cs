using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// A stack implementation that uses NativeList internally
    /// </summary>
    public struct NativeStack<T> where T : struct {
        private NativeList<T> internalList;

        public NativeStack(int initialCapacity, Allocator allocator) {
            this.internalList = new NativeList<T>(initialCapacity, allocator);
        }

        public void Clear() {
            this.internalList.Clear();
        }

        public void Push(T item) {
            this.internalList.Add(item);
        }

        public T Pop() {
            T top = this.Top;
            this.internalList.RemoveAt(this.internalList.Length - 1); // Remove last item

            return top;
        }

        public T Top {
            get {
                return this.internalList[this.internalList.Length - 1];
            }
        }

        public int Count {
            get {
                return this.internalList.Length;
            }
        }
        
        public int Length {
            get {
                return this.internalList.Length;
            }
        }

        public void Dispose() {
            if (this.internalList.IsCreated) {
                this.internalList.Dispose();
            }
        }
    }
}