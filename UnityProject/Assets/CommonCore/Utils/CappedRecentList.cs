using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// A custom list that has a cap. Old entries are removed.
    /// This is usually used for graph node data where old data are discarded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CappedRecentList<T> {
        private readonly List<T> list;
        private readonly int cap;

        public CappedRecentList(int cap) {
            this.list = new List<T>(cap);
            
            this.cap = cap;
            Assertion.IsTrue(this.cap > 0);
        }
        
        public T this[int index] {
            get {
                return this.list[index];
            }
        }

        public int Count {
            get {
                return this.list.Count;
            }
        }

        public void Clear() {
            this.list.Clear();
        }

        public void Add(T item) {
            // Remove older items if cap has been reached
            while (this.list.Count + 1 > cap) {
                this.list.RemoveAt(0);
            }
            
            this.list.Add(item);
        }
    }
}