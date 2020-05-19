using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /**
     * A general class that manages prefab instances.
     */
    public class PrefabManager : MonoBehaviour {
        [SerializeField]
        private SelfManagingSwarmItemManager itemManager;

        private IDictionary<string, int> nameToIndexMapping;

        [SerializeField]
        private PreloadData[] preloadDataList;

        [SerializeField]
        private PruneData[] pruneDataList;

        [SerializeField]
        private float pruneIntervalTime = 1.0f;

        public SelfManagingSwarmItemManager ItemManager {
            get {
                return this.itemManager;
            }
        }

        /**
         * Returns the number of prefabs that the manager can instantiate
         */
        public int PrefabCount {
            get {
                return this.itemManager.itemPrefabs.Length;
            }
        }

        private void Awake() {
            Assertion.NotNull(this.itemManager);

            // populate moduleMapping
            this.nameToIndexMapping = new Dictionary<string, int>();
            for (int i = 0; i < this.itemManager.itemPrefabs.Length; ++i) {
                SwarmItemManager.PrefabItem current = this.itemManager.itemPrefabs[i];
                this.nameToIndexMapping[current.prefab.name] = i;
            }

            StartCoroutine(Preload());
        }

        private IEnumerator Preload() {
            // give chance for SwarmItemManager to prepare itself
            yield return true;

            if (this.preloadDataList == null || this.preloadDataList.Length == 0) {
                // nothing to preload
                yield break;
            }

            for (int i = 0; i < this.preloadDataList.Length; ++i) {
                Preload(this.preloadDataList[i].PrefabName, this.preloadDataList[i].PreloadCount);
            }
        }

        /**
         * Prunes inactive items
         * Should be invoked at certain time in the game where frame rate is not important like transitioning to another screen
         */
        public void Prune() {
            // pruning is done across several frames
            StartCoroutine(PruneItems());
        }

        private IEnumerator PruneItems() {
            foreach (PruneData data in this.pruneDataList) {
                int itemPrefabIndex = this.nameToIndexMapping[data.PrefabName];
                this.itemManager.PruneInactiveList(itemPrefabIndex, data.MaxInactiveCount);

                yield return 0; // distribute pruning in different frames so that it won't hog down runtime
            }
        }

        /**
         * Return whether or not the PrefabManager contains the specified prefab
         */
        public bool ContainsPrefab(string prefabName) {
            return this.nameToIndexMapping.ContainsKey(prefabName);
        }

        /**
         * Requests for a prefab instance.
         */
        public GameObject Request(string prefabName) {
            Assertion.IsTrue(this.nameToIndexMapping.ContainsKey(prefabName),
                prefabName); // "nameToIndexMapping should contain the specified prefab name: " + prefabName
            int prefabIndex = this.nameToIndexMapping[prefabName];

            return Request(prefabIndex);
        }

        /**
         * Requests for a prefab instance with specified position and orientation.
         */
        public GameObject Request(string prefabName, Vector3 position, Quaternion rotation) {
            GameObject instantiated = Request(prefabName);
            instantiated.transform.position = position;
            instantiated.transform.rotation = rotation;

            return instantiated;
        }

        /**
         * Requests for a prefab instance using the specified index
         */
        public GameObject Request(int prefabIndex) {
            SwarmItem item = this.itemManager.ActivateItem(prefabIndex);

            return item.gameObject;
        }

        /**
         * Preloads the specified prefab for a certain amount
         */
        public void Preload(string prefabName, int count) {
            Assertion.IsTrue(
                this.nameToIndexMapping.
                    ContainsKey(
                        prefabName)); // "nameToIndexMapping should contain the specified prefab name: " + prefabName
            int prefabIndex = this.nameToIndexMapping[prefabName];
            this.itemManager.Preload(prefabIndex, count);
        }

        /// <summary>
        ///     Returns the inactive count of the specified prefab
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public int GetInactiveCount(string prefabName) {
            Assertion.IsTrue(
                this.nameToIndexMapping.
                    ContainsKey(
                        prefabName)); // "nameToIndexMapping should contain the specified prefab name: " + prefabName
            int prefabIndex = this.nameToIndexMapping[prefabName];

            return this.itemManager.GetInactiveCount(prefabIndex);
        }

        /// <summary>
        ///     Reserves a certain number instances of the specified prefab
        ///     This is usually used for heavy prefabs. Client may want to reserve some instances so that the game
        ///     will not be bogged down.
        ///     This is different from preload. Reserve instantiates new instances instead of reusing the current inactive
        ///     ones.
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="count"></param>
        public void Reserve(string prefabName, int count) {
            Assertion.IsTrue(
                this.nameToIndexMapping.
                    ContainsKey(
                        prefabName)); // "nameToIndexMapping should contain the specified prefab name: " + prefabName
            int prefabIndex = this.nameToIndexMapping[prefabName];
            this.itemManager.Reserve(prefabIndex, count);
        }

        /**
         * Kills all active items.
         */
        public void KillAllActiveItems() {
            this.itemManager.KillAllActiveItems();
        }

        /**
         * Sets the prune data list. May be invoked in editor when populating the prune data.
         */
        public void SetPruneDataList(PruneData[] pruneDataList) {
            this.pruneDataList = pruneDataList;
        }

        /**
         * Sets the preload data list
         */
        public void SetPreloadDataList(PreloadData[] dataList) {
            this.preloadDataList = dataList;
        }

        /**
         * A utility data that describes pruning process of each prefab in the prefab manager.
         */
        [Serializable]
        public class PruneData {

            [SerializeField]
            private int maxInactiveCount;

            [SerializeField]
            private string prefabName;

            public string PrefabName {
                get {
                    return this.prefabName;
                }
            }

            public int MaxInactiveCount {
                get {
                    return this.maxInactiveCount;
                }
            }

            /**
             * Sets the values of the data
             */
            public void Set(string prefabName, int maxInactiveCount) {
                this.prefabName = prefabName;
                this.maxInactiveCount = maxInactiveCount;
            }
        }

        /**
         * A data class that contains information on how prefab instances are preloaded
         */
        [Serializable]
        public class PreloadData {

            [SerializeField]
            private string prefabName;

            [SerializeField]
            private int preloadCount;

            public string PrefabName {
                get {
                    return this.prefabName;
                }
            }

            public int PreloadCount {
                get {
                    return this.preloadCount;
                }
            }

            /**
             * Sets the values of the data class
             */
            public void Set(string prefabName, int preloadCount) {
                this.prefabName = prefabName;
                this.preloadCount = preloadCount;
            }
        }
    }
}

