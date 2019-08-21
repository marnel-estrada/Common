using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// The same as DataClassPool but implemented as ScriptableObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPool<T> : ScriptableObject where T : Identifiable, IntIdentifiable, new() {
        [SerializeField]
        private List<T> dataList = new List<T>();

        [SerializeField]
        private IdGenerator idGenerator = new IdGenerator();
        
        private Dictionary<string, T> map;

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

            if (this.map == null) {
                this.map = new Dictionary<string, T>(10);
            }

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
        public Maybe<T> Find(string id) {
            PopulateMap(); // We populate first because this may be invoked before Awake() (like in editor)
            return new Maybe<T>(this.map.Find(id));
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
        public T AddNew(string id) {
            T data = new T();
            data.IntId = this.idGenerator.Generate();
            data.Id = id;
            
            this.dataList.Add(data);
            this.map[data.Id] = data;

            return data;
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
                if(item.Id.EqualsFast(id)) {
                    this.removeList.Add(item);
                }
            }

            for(int i = 0; i < this.removeList.Count; ++i) {
                this.dataList.Remove(this.removeList[i]);
            }

            this.removeList.Clear();
        }

        public void Sort(Comparison<T> comparison) {
            this.dataList.Sort(comparison);
        }
    }
}