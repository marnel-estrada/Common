using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// A container that manages items both in a list and dictionary
    /// </summary>
    public class ListDictionary<K, V> {
        private readonly List<K> keyList = new List<K>();
        private readonly List<V> valueList = new List<V>();
        private readonly Dictionary<K, V> dictionary = new Dictionary<K, V>();

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(K key, V value) {
            Assertion.IsTrue(!this.dictionary.ContainsKey(key)); // should not have the said item in the container

            this.keyList.Add(key);
            this.valueList.Add(value);
            this.dictionary.Add(key, value);

            Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);
        }

        /// <summary>
        /// Changes the value for the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(K key, V value) {
            this.dictionary[key] = value;

            // Change in lists as well
            int index = this.keyList.IndexOf(key);
            this.valueList[index] = value;
        }

        /// <summary>
        /// Returns whether or not the container contains the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(K key) {
            return this.dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Removes an item with the specified key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(K key) {
            int listIndex = this.keyList.IndexOf(key);
            if (listIndex < 0) {
                Debug.LogError("Key not found: " + key);
                return;
            }
            
            this.keyList.RemoveAt(listIndex);
            this.valueList.RemoveAt(listIndex);

            this.dictionary.Remove(key);

            Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);
        }

        /// <summary>
        /// Removes the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index) {
            K key = this.keyList[index];
            this.keyList.RemoveAt(index);
            this.valueList.RemoveAt(index);
            this.dictionary.Remove(key);

            Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);
        }

        /// <summary>
        /// Retrieves a value using an index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public V GetAt(int index) {
            // This throws error if index is out of bounds
            return this.valueList[index];
        }

        /// <summary>
        /// Clears the container of all items
        /// </summary>
        public void Clear() {
            this.keyList.Clear();
            this.valueList.Clear();
            this.dictionary.Clear();

            Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);
        }

        /// <summary>
        /// Returns the number of items in the container
        /// </summary>
        public int Count {
            get {
                Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);
                return this.dictionary.Count;
            }
        }

        public IReadOnlyList<V> ListEntries {
            get {
                return this.valueList;
            }
        }

        public IEnumerable<KeyValuePair<K, V>> KeyValueEntries {
            get {
                return this.dictionary;
            }
        }

        public int IndexOfKey(K key) {
            Assertion.IsTrue(this.keyList.Count == this.valueList.Count && this.valueList.Count == this.dictionary.Count);

            return this.keyList.IndexOf(key);
        }
    }
}
