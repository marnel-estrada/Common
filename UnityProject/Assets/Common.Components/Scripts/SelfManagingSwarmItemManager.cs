using System.Collections.Generic;

namespace Common {
    /**
     * A custom SwarmItemManager for Shopping game.
     */
    public class SelfManagingSwarmItemManager : SwarmItemManager {
        // we need this to store activated SwarmItem temporarily
        // they will be killed right after
        // we can't Activate() then Kill() because it will only reuse the killed item
        private readonly SimpleList<SwarmItem> preloadList = new SimpleList<SwarmItem>();

        private void Awake() {
            Initialize();
        }

        /**
         * Kills all active items.
         */
        public void KillAllActiveItems() {
            int length = this._prefabItemLists.Length;
            for (int i = 0; i < length; ++i) {
                // only bother if the active list has some items
                if (this._prefabItemLists[i].activeItems.Count <= 0) {
                    continue;
                }

                // we don't iterate through the active list using foreach here
                // because there would be errors if the item was killed in its FrameUpdate method.
                // instead we manually move to the next linkedlist nod/

                LinkedListNode<SwarmItem>? item = this._prefabItemLists[i].activeItems.First;

                // while there are items left to process
                while (item != null) {
                    // cache the next item because the current item will be killed
                    LinkedListNode<SwarmItem>? nextItem = item.Next;
                    DeactiveItem(item.Value);
                    item = nextItem;
                }
            }
        }

        /**
         * Prunes the inactive list to the specified max count.
         */
        public void PruneInactiveList(int itemPrefabIndex, int maxCount) {
            Assertion.IsTrue(maxCount > 0, "Max count can't be zero.");

            int numToPrune = this._prefabItemLists[itemPrefabIndex].inactiveItems.Count - maxCount;
            if (numToPrune <= 0) {
                // nothing to prune
                // the number of inactive items does not even reach the maxCount
                return;
            }

            PruneList(itemPrefabIndex, numToPrune);
        }

        /**
         * Preloads a certain prefab to a certain amount
         */
        public void Preload(int itemPrefabIndex, int preloadCount) {
            if (this._prefabItemLists[itemPrefabIndex].inactiveItems.Count >= preloadCount) {
                // there are already enough inactive items
                // no need to proceed
                return;
            }

            this.preloadList.Clear();

            // note here that we only preload the remaining amount
            // to preload, we need to passes
            // first to activate items
            // then another pass to kill them
            // we can't simple Activate() then Kill() because it will just reuse the previously killed unit
            int remaining = preloadCount - this._prefabItemLists[itemPrefabIndex].inactiveItems.Count;
            for (int i = 0; i < remaining; ++i) {
                SwarmItem item = ActivateItem(itemPrefabIndex);
                this.preloadList.Add(item); // add temporarily so we could kill later
            }

            for (int i = 0; i < this.preloadList.Count; ++i) {
                this.preloadList[i].Kill();
            }

            this.preloadList.Clear();
        }

        /// <summary>
        ///     Reserves a certain number of instances
        /// </summary>
        /// <param name="itemPrefabIndex"></param>
        /// <param name="reserveCount"></param>
        public void Reserve(int itemPrefabIndex, int reserveCount) {
            for (int i = 0; i < reserveCount; ++i) {
                // Note here that we force instantiate. If not, it will just reuse an inactive item thus
                // renders the reservation meaningless.
                SwarmItem item = ActivateItem(itemPrefabIndex, true);
                item.Recycle(); // Recycle right away as we are only reserving the instance
            }
        }

        /// <summary>
        ///     Returns the inactive count
        ///     It can also be considered as the reserved count
        /// </summary>
        /// <param name="prefabIndex"></param>
        /// <returns></returns>
        public int GetInactiveCount(int prefabIndex) {
            return this._prefabItemLists[prefabIndex].inactiveItems.Count;
        }
    }
}

