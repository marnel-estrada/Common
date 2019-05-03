using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// A generic container that collects items in a list and has an ID.
    /// </summary>
    public class IdentifiableSet<T> : Identifiable {

        private string id;
        private readonly List<T> list;

        /// <summary>
        /// Constructor with specified ID
        /// </summary>
        /// <param name="id"></param>
        public IdentifiableSet(string id) {
            this.id = id;
            this.list = new List<T>();
        }

        public string Id {
            get {
                return this.id;
            }

            set {
                this.id = value;
            }
        }
        
        /// <summary>
        /// Adds an item to the set
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            this.list.Add(item);
        }

        public int Count {
            get {
                return this.list.Count;
            }
        }

        /// <summary>
        /// Returns the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetAt(int index) {
            return this.list[index];
        }

    }
}
