using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// The same as DataClassPool but implemented as ScriptableObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataPool<T> : ScriptableObject where T : IDataPoolItem, IDuplicable<T>, new() {
        // This is used for the editor such that we don't need to resolve it by path
        [SerializeField]
        private GUISkin guiSkin;
        
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
            T found = this.map.Find(id);
            return found == null ? Maybe<T>.Nothing : new Maybe<T>(found);
        }

        /// <summary>
        /// Searches for an item using the integer ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Maybe<T> Find(int id) {
            PopulateMap();
            for (int i = 0; i < this.dataList.Count; ++i) {
                T item = this.dataList[i];
                if (item.IntId == id) {
                    return new Maybe<T>(item);
                }
            }
            
            return Maybe<T>.Nothing;
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

        public GUISkin Skin {
            get {
                return this.guiSkin;
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

        /// <summary>
        /// Adds an instantiated item
        /// We did it this way so we can maintain the int id
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            Assertion.Assert(!this.map.ContainsKey(item.Id)); // Should not contain the same ID
            
            item.IntId = this.idGenerator.Generate();
            this.dataList.Add(item);
            this.map[item.Id] = item;
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
                this.map.Remove(this.removeList[i].Id); // Remove from dictionary as well
            }

            this.removeList.Clear();
        }

        public void Sort(Comparison<T> comparison) {
            this.dataList.Sort(comparison);
        }
    }
}