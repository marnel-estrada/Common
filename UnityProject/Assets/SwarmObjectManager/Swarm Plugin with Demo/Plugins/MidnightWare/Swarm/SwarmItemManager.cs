//-----------------------------------------------------------------
//  Copyright 2011 Layne Bryant   echo17
//-----------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///     The SwarmItemManager handles SwarmItem objects. It actively recycles SwarmItems so that no garbage
///     collection is necessary. It will dynamically add new objects as required, pulling from inactive lists
///     if any recycled SwarmItems are available. You can have multiple SwarmItemManagers in a scene to handle
///     different types of objects. Alternatively, you could handle all your object types within a single
///     SwarmItemManager.
///     The manager can limit each of its lists to an optional maximum item count. If this limit greater than zero,
///     then the manager won't instantiate any more objects in a list once it has reached the maximum count. If the
///     limit is zero, the manager will keep instantiating new objects as neccessary.
///     Each item list has its own optional pruning functionality. When an item is pruned it is sent to the garbage
///     collector.
///     If the number of the inactive items of a prefab exceed the set threshold, a pruning timer will kick in.
///     If the number of the inactive items drop back below the threshold, the pruning timer will shut back off.
///     When the pruning timer countdown expires, the inactive list will be pruned to the prune percentage set
///     for the item. You can shut off pruning by setting the inactivePrunePercentage to zero.
/// </summary>
public class SwarmItemManager : MonoBehaviour {

	/// <summary>
	///     internal member that stores the parent transform of all active items. This is used as a visual aid in the editor.
	/// </summary>
	private Transform? activeParentTransform;

	/// <summary>
	///     internal member that stores the parent transform of all inactive items. This is used as a visual aid in the editor.
	/// </summary>
	private Transform _inactiveParentTransform;

	/// <summary>
	///     internal member to set up a SwarmItem
	/// </summary>
	private SwarmItem _item;

	/// <summary>
	///     internal member that keeps track of the total items created
	/// </summary>
	private int _itemCount;

	/// <summary>
	///     the array of prefab item lists. Each prefab gets a set of inactive and active lists
	/// </summary>
	protected PrefabItemLists[] _prefabItemLists;

	/// <summary>
	///     internal member to cache a SwarmItem's transform
	/// </summary>
	private Transform _transform;

	/// <summary>
	///     flag to show important events happening in this manager
	/// </summary>
	public bool debugEvents;

	/// <summary>
	///     the array of prefabs with their maximum item counts. This is set in the editor
	/// </summary>
	public PrefabItem[] itemPrefabs;

	/// <summary>
	///     Sets up the lists for each SwarmItem type. Also creates the parent transform for the active and inactive objects
	/// </summary>
	public virtual void Initialize() {
        // warn the user if no prefabs were set up. There would be no need for a manager without SwarmItems
        if (this.itemPrefabs.Length == 0) {
            Debug.Log("WARNING! No Item Prefabs exists for " + this.gameObject.name + " -- Errors will occur.",
                this.gameObject);
        }

        // make sure all the thresholds and percentages are clamped between 0 and 1.0f
        foreach (PrefabItem itemPrefab in this.itemPrefabs) {
            itemPrefab.inactiveThreshold = Mathf.Clamp01(itemPrefab.inactiveThreshold);
            itemPrefab.inactivePrunePercentage = Mathf.Clamp01(itemPrefab.inactivePrunePercentage);
        }

        // initialize the prefab item lists
        this._prefabItemLists = new PrefabItemLists[this.itemPrefabs.Length];
        for (int i = 0; i < this._prefabItemLists.Length; i++) {
            this._prefabItemLists[i] = new PrefabItemLists();
            this._prefabItemLists[i].inactivePruneTimeLeft = 0;
        }

        // create the active objects parent transform
        PrepareActiveItems();

        // create the inactive objects parent transform;
        PrepareInactiveItems();
	}

	private void PrepareInactiveItems() {
		GameObject go = new GameObject("Inactive Items");
		this._inactiveParentTransform = go.transform;
		this._inactiveParentTransform.parent = this.transform;
		this._inactiveParentTransform.localPosition = Vector3.zero;
		this._inactiveParentTransform.localRotation = Quaternion.identity;
		this._inactiveParentTransform.localScale = Vector3.one;
	}

	private void PrepareActiveItems() {
		if (this.activeParentTransform != null) {
			// Already prepared
			return;
		}
		
		GameObject go = new GameObject("Active Items");
		this.activeParentTransform = go.transform;
		this.activeParentTransform.SetParent(this.transform);
		this.activeParentTransform.localPosition = Vector3.zero;
		this.activeParentTransform.localRotation = Quaternion.identity;
		this.activeParentTransform.localScale = Vector3.one;
	}

	/// <summary>
	///     Overloaded form of ActivateItem that assumes you just want the first SwarmItem type (first prefab)
	/// </summary>
	/// <returns>Returns the newly created SwarmItem</returns>
	public virtual SwarmItem ActivateItem() {
        return ActivateItem(0);
    }

	/// <summary>
	///     Activates a SwarmItem base on the prefab index (type). If there is an inactive item,
	///     the manager will recycle that first, otherwise it will instantiate a new item
	/// </summary>
	/// <param name="itemPrefabIndex">The index of the prefab to use (type of SwarmItem)</param>
	/// <param name="forceInstantiate">If set to true, it instantiates the item instead of reusing</param>
	/// <returns>Returns the newly created SwarmItem</returns>
	public virtual SwarmItem ActivateItem(int itemPrefabIndex, bool forceInstantiate = false) {
        // we have exceeded the maximum item count for this prefab (if a limit is set)
        // so return nothing
        if (this._prefabItemLists[itemPrefabIndex].activeItems.Count ==
            this.itemPrefabs[itemPrefabIndex].maxItemCount && this.itemPrefabs[itemPrefabIndex].maxItemCount > 0) {
            if (this.debugEvents) {
                Debug.Log("Could not activate item because the count [" +
                    this._prefabItemLists[itemPrefabIndex].activeItems.Count +
                    "] is at the maximum number for this item type at frame: " + Time.frameCount);
            }

            return null;
        }

        SwarmItem localItem = null;

        // [Marnel] HACK!!!
        // search for a non null item while there are inactive items
        // for some reason, the entry in the inactiveItems becomes null
        while (!forceInstantiate && localItem == null &&
            this._prefabItemLists[itemPrefabIndex].inactiveItems.Count > 0) {
            localItem = this._prefabItemLists[itemPrefabIndex].inactiveItems.Pop();
        }

        if (localItem == null || forceInstantiate) {
            // no item to recycle
            // instantiate item
            localItem = InstantiateItem(itemPrefabIndex);

            // queue to the end of the active list
            this._prefabItemLists[itemPrefabIndex].activeItems.AddLast(localItem);

            if (this.debugEvents) {
                Debug.Log("Instantiated a new item " + localItem.gameObject.name + " at frame: " + Time.frameCount);
            }
        } else {
            // there is an inactive item so we recycle it
            this._prefabItemLists[itemPrefabIndex].activeItems.AddLast(localItem);

            if (this.debugEvents) {
                Debug.Log("Recycled item " + localItem.name + " at frame: " + Time.frameCount);
            }
        }

        if (this.activeParentTransform == null) {
	        PrepareActiveItems();
        }

        // move the item to active parent transform.
        // this is mainly just for visual reference in the editor
        SetItemParentTransform(localItem, this.activeParentTransform);

        // set the state to active
        localItem.State = SwarmItem.STATE.Active;

        // if the prune timer is runnning
        if (this._prefabItemLists[itemPrefabIndex].inactivePruneTimeLeft > 0) {
            // if the inactive item count dropped below the threshold
            if (this._prefabItemLists[itemPrefabIndex].inactiveItems.Count /
                (float) this._prefabItemLists[itemPrefabIndex].ItemCount <
                this.itemPrefabs[itemPrefabIndex].inactiveThreshold) {
                if (this.debugEvents) {
                    Debug.Log("Dropped below inactive threshold [" +
                        this.itemPrefabs[itemPrefabIndex].inactiveThreshold * 100 + "%] for " +
                        this.itemPrefabs[itemPrefabIndex].prefab.name +
                        " list before timer expired. Stopping prune timer at frame: " + Time.frameCount);
                }

                // turn the prune timer off
                this._prefabItemLists[itemPrefabIndex].inactivePruneTimeLeft = 0;
            }
        }

        return localItem;
    }

	/// <summary>
	///     Moves a SwarmItem from the active list to the inactive list and changes its parent transform
	/// </summary>
	/// <param name="item">The SwarmItem to deactivate</param>
	public virtual void DeactiveItem(SwarmItem item) {
        if (item == null) {
            throw new Exception("SwarmItem can't be null");
        }

        // remove from the active linked list
        this._prefabItemLists[item.PrefabIndex].activeItems.Remove(item);

        // push onto the inactive stack
        this._prefabItemLists[item.PrefabIndex].inactiveItems.Push(item);

        // move the item to the inactive parent transform.
        // this is mainly just for visual reference in the editor
        SetItemParentTransform(item, this._inactiveParentTransform);

        if (this.debugEvents) {
            Debug.Log("Deactivated " + item.name + " at frame: " + Time.frameCount);
        }

        // if the prune timer is not currently running and we actually want to prune
        if (this._prefabItemLists[item.PrefabIndex].inactivePruneTimeLeft == 0 &&
            this.itemPrefabs[item.PrefabIndex].inactivePrunePercentage > 0) {
            // if the inactive item count exceeds the threshold
            if (this._prefabItemLists[item.PrefabIndex].inactiveItems.Count /
                (float) this._prefabItemLists[item.PrefabIndex].ItemCount >=
                this.itemPrefabs[item.PrefabIndex].inactiveThreshold) {
                if (this.debugEvents) {
                    Debug.Log("Inactive threshold [" + this.itemPrefabs[item.PrefabIndex].inactiveThreshold * 100 +
                        "%] reached for " + this.itemPrefabs[item.PrefabIndex].prefab.name +
                        " list. Starting prune timer [" + this.itemPrefabs[item.PrefabIndex].inactivePruneTimer +
                        " seconds] at frame: " + Time.frameCount);
                }

                // if the prune timer is set to expire immediately
                if (this.itemPrefabs[item.PrefabIndex].inactivePruneTimer == 0) {
                    // don't wait for a countdown, just prune immediately
                    PruneList(item.PrefabIndex, this.itemPrefabs[item.PrefabIndex].inactivePrunePercentage);
                } else {
                    // turn the prune timer on
                    this._prefabItemLists[item.PrefabIndex].inactivePruneTimeLeft =
                        this.itemPrefabs[item.PrefabIndex].inactivePruneTimer;
                }
            }
        }
    }

	/// <summary>
	///     Creates a new gameobject for the SwarmItem and initializes it
	/// </summary>
	/// <param name="itemPrefabIndex">The index of the prefab to use (type of SwarmItem)</param>
	/// <returns>Returns the newly created SwarmItem</returns>
	protected virtual SwarmItem InstantiateItem(int itemPrefabIndex) {
        SwarmItem item;

        // instantiate
        GameObject go = Instantiate(this.itemPrefabs[itemPrefabIndex].prefab);
        // change the name of the gameobject with an index and take off the 'Clone' postfix
        go.name = "[" + this._itemCount.ToString("0000") + "] " + go.name.Replace("(Clone)", "");

        // get the SwarmItem component from the gameobject
        item = (SwarmItem) go.GetComponent(typeof(SwarmItem));
        Assert.IsNotNull(item, "SwarmItem");

        // initialize the SwarmItem
        item.Initialize(this, itemPrefabIndex, this.debugEvents);

        // increment the total item count for this manager
        this._itemCount++;

        return item;
    }

	/// <summary>
	///     Moves a SwarmItem to the active or inactive transforms.
	///     This is mainly used a visual aid in the editor to see which items are active or inactive
	/// </summary>
	/// <param name="item">The SwarmItem to move</param>
	/// <param name="parentTransform">The parent transform to move to</param>
	private static void SetItemParentTransform(SwarmItem item, Transform parentTransform) {
        if (item == null) {
            throw new Exception("item can't be null");
        }

        if (parentTransform == null) {
            throw new Exception("parentTransform can't be null");
        }

        // reparent this item's transform
        item.ThisTransform.SetParent(parentTransform, false);

        // reset the position, rotation, and scale to unit values
        item.ThisTransform.localPosition = Vector3.zero;
        item.ThisTransform.localRotation = Quaternion.identity;
        item.ThisTransform.localScale = Vector3.one;

        // if the position, rotation, or scale need to be changed after reparenting, do it in the
        // item's OnSetParentTransform method
        item.OnSetParentTransform();
    }

	/// <summary>
	///     This function is called every frame. It will iterate through each SwarmItem type's active list,
	///     calling FrameUpdate on each of the items. This method should be called from a central Update() Mono
	///     method, or you can add an Update() method here and call it.
	/// </summary>
	public virtual void FrameUpdate() {
        // iterate through each SwarmItem type
        for (int i = 0; i < this._prefabItemLists.Length; i++) {
            // if this list has its prune timer turned on
            if (this._prefabItemLists[i].inactivePruneTimeLeft > 0) {
                // decrement the prune timer
                this._prefabItemLists[i].inactivePruneTimeLeft -= Time.deltaTime;

                // if the timer has expired
                if (this._prefabItemLists[i].inactivePruneTimeLeft <= 0) {
                    // prune the list
                    PruneList(i, this.itemPrefabs[i].inactivePrunePercentage);
                }
            }
        }
    }

	/// <summary>
	///     Removes inactive items from the list after the inactive item count exceeds a threshold and
	///     no new items are activated from the list before the prune timer countdown expires. Alternatively,
	///     you could call this manually to free up memory at any time.
	/// </summary>
	/// <param name="itemPrefabIndex">
	///     The index of the list to prune
	/// </param>
	/// <param name="prunePercentage">
	///     The amount (relative to the number of inactive items) to prune from the inactive list.
	/// </param>
	public void PruneList(int itemPrefabIndex, float prunePercentage) {
        // turn off the prune timer
        this._prefabItemLists[itemPrefabIndex].inactivePruneTimeLeft = 0;

        // get the number of items to prune based on the prune percentage.
        // the amount is a percentage of the inactive items, not the total item count for this list
        int pruneCount = Mathf.FloorToInt(prunePercentage * this._prefabItemLists[itemPrefabIndex].inactiveItems.Count);
        PruneList(itemPrefabIndex, pruneCount);
    }

	/// <summary>
	///     A convenience method to prune the inactive list with the specified pruneCount. This is a hack so it might get
	///     overridden when updates to the plugin is made.
	/// </summary>
	/// <param name='itemPrefabIndex'>
	///     Item prefab index.
	/// </param>
	/// <param name='pruneCount'>
	///     Prune count.
	/// </param>
	protected void PruneList(int itemPrefabIndex, int pruneCount) {
        SwarmItem item;

        if (this.debugEvents) {
            Debug.Log("Pruning " + pruneCount + " items [" +
                this.itemPrefabs[itemPrefabIndex].inactivePrunePercentage * 100 + "% of " +
                this._prefabItemLists[itemPrefabIndex].inactiveItems.Count + "] from inactive " +
                this.itemPrefabs[itemPrefabIndex].prefab.name + " list at frame: " + Time.frameCount);
        }

        // prune each item
        while (pruneCount > 0) {
            // pop an item from the inactive stack
            item = this._prefabItemLists[itemPrefabIndex].inactiveItems.Pop();

            // call the overloaded PreDestroy function to let the inherited objects
            // free any memory
            item.PreDestroy();

            if (this.debugEvents) {
                Debug.Log("Destroyed " + item.name + " at frame: " + Time.frameCount);
            }

            // destroy the item
            Destroy(item.gameObject);
            item = null;

            // decrement this list's item count and the manager's total item count
            this._itemCount--;

            // move to the next item to prune
            pruneCount--;
        }
    }

	/// <summary>
	///     This internal class represents the active and inactive lists of a SwarmItem type.
	///     Since SwarmItemManager's can handle multiple types of SwarmItems, there is
	///     an active and inactive list for each.
	/// </summary>
	protected class PrefabItemLists {
		/// <summary>
		///     linked list of active items. Faster than a List since we may need to remove items in the middle of the list
		/// </summary>
		public LinkedList<SwarmItem> activeItems;

		/// <summary>
		///     stack of inactive items. It doesn't matter which of the inactive items we pull, so we always pop the top for
		///     efficiency
		/// </summary>
		public Stack<SwarmItem> inactiveItems;

		/// <summary>
		///     the amount of time in seconds left before the inactive list is pruned
		///     (unless the number of inactive items drops below the threshold)
		/// </summary>
		public float inactivePruneTimeLeft;

		/// <summary>
		///     Initializes the lists
		/// </summary>
		public PrefabItemLists() {
            this.activeItems = new LinkedList<SwarmItem>();
            this.inactiveItems = new Stack<SwarmItem>();
        }

		/// <summary>
		///     the total number of items (active and inactive in this list)
		/// </summary>
		//public int itemCount;

        public int ItemCount {
            get {
                return this.activeItems.Count + this.inactiveItems.Count;
            }
        }
    }

	/// <summary>
	///     This class is used to wrap the prefab so that you can set the maximum
	///     count for each item and set the pruning parameters, both optional.
	/// </summary>
	[Serializable]
    public class PrefabItem {

		/// <summary>
		///     the percentage to prune the inactive list when the prune timer countdown expires.
		///     the amount is the percentage of the inactive list, not the total item count.
		///     (ex: 0.3f = approximately one third of the inactive items will be destroyed when the prune timer expires).
		///     defaults to no pruning.
		/// </summary>
		public float inactivePrunePercentage;

		/// <summary>
		///     the amount of time in seconds to count down after the prune threshold is exceeded.
		///     if the inactive count drops below the threshold before this timer has expired, the
		///     timer will be shut off and reset.
		///     (ex: 5.0f = 5 seconds after the prune threshold is exceeded, the inactive list will be pruned).
		///     defaults to pruning immediately when threshold is reached.
		/// </summary>
		public float inactivePruneTimer;

		/// <summary>
		///     the percentage of total items in the list that triggers the prune timer countdown.
		///     if the inactive item count exceeds this percentage, the timer is triggered.
		///     (ex: 0.7f = when the inactive item count is greater than 70% of all the items (active and inactive)
		///     the prune timer will kick in).
		///     defaults to turning on the pruning timer only when every item is inactive.
		/// </summary>
		public float inactiveThreshold = 1.0f;

		/// <summary>
		///     maximum count for this item (0 = no limit, postive number = the list will be limited to the max item count)
		/// </summary>
		public int maxItemCount;

		/// <summary>
		///     prefab game object of the item
		/// </summary>
		public GameObject prefab;
    }
}