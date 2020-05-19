using System.Collections.Generic;

using UnityEngine;

namespace Common.Utils {
    /// <summary>
    /// Maintains a pool of T instances
    /// </summary>
    public class DataClassPool<T> : MonoBehaviour where T : Identifiable {
        [SerializeField]
        private List<T> dataList = new List<T>();

        private Dictionary<string, T> map = new Dictionary<string, T>();

        /// <summary>
        /// Awake routines
        /// </summary>
        public virtual void Awake() {
            if (this.dataList.Count > 0) {
                PopulateMap();
            }
        }

        private void PopulateMap() {
            if(this.map != null && this.map.Count > 0) {
                // Already populated
                return;
            }

            this.map = new Dictionary<string, T>();
            for (int i = 0; i < this.dataList.Count; ++i) {
                T data = this.dataList[i];
                this.map[data.Id] = data;
            }
        }

        /// <summary>
        /// Looks for the instance with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Find(string id) {
            PopulateMap(); // We populate first because this may be invoked before Awake() (like in editor)

            Assertion.IsTrue(this.map.TryGetValue(id, out T data), id); // data should exist

            return data;
        }

        /// <summary>
        /// Returns whether or not the data pool contains the specified item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(string id) {
            PopulateMap(); // We populate first because this may be invoked before Awake() (like in editor)
            return this.map.ContainsKey(id);
        }

        /// <summary>
        /// Returns the number of items in the pool
        /// </summary>
        public int Count {
            get {
                return this.dataList.Count;
            }
        }

        /// <summary>
        /// Returns the instance at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetAt(int index) {
            return this.dataList[index];
        }

        /// <summary>
        /// Returns all instances of items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll() {
            return this.dataList;
        }

        /// <summary>
        /// Adds a new item
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            this.dataList.Add(item);
        }

        private readonly SimpleList<T> removeList = new SimpleList<T>();

        /// <summary>
        /// Removes the specified item
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string id) {
            this.removeList.Clear();

            // We search through list because IDs may repeat
            for(int i = 0; i < this.dataList.Count; ++i) {
                T item = this.dataList[i];
                if(item.Id.Equals(id)) {
                    this.removeList.Add(item);
                }
            }

            for(int i = 0; i < this.removeList.Count; ++i) {
                this.dataList.Remove(this.removeList[i]);
            }

            this.removeList.Clear();
        }
    }
}
