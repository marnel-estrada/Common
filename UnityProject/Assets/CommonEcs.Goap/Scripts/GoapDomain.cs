using System;
using Unity.Collections;

namespace CommonEcs.Goap {
    public struct GoapDomain {
        public readonly FixedString64 name;

        // This is the list of actions
        public GoapActionList64 actions;

        // The list of integers mapped to the condition are indices to actions
        public FixedHashMap<Condition, FixedList64<int>> actionMap;

        public int actionsCount;

        public GoapDomain(FixedString64 name) : this() {
            this.name = name;
        }

        public void AddAction(in GoapAction action) {
            this.actions[this.actionsCount] = action;

            // Add to action map
            FixedList32<int> indexList = ResolveFixedList(action.effect);
            indexList.Add(this.actionsCount);

            // We need to do insertion sort here because we need to sort the actions by cost
            // Bubble down the added action until its cost is in the right place
            for (int i = indexList.Length - 1; i > 0; --i) {
                float currentCost = this.actions[indexList[i]].cost;
                float previousCost = this.actions[indexList[i - 1]].cost;

                if (currentCost.TolerantGreaterThanOrEquals(previousCost)) {
                    // This means that the action at i is already in its correct place
                    break;
                }

                // At this point, this means that the action at i has lesser cost than the action
                // at i - 1
                // We swap action indices
                (indexList[i], indexList[i - 1]) = (indexList[i - 1], indexList[i]);
            }

            // Update map (because we are using structs)
            this.actionMap.AddOrSet(action.effect, indexList);
            ++this.actionsCount;
        }

        private FixedList32<int> ResolveFixedList(in Condition effect) {
            ValueTypeOption<FixedList64<int>> found = this.actionMap.Find(effect);
            if (found.IsSome) {
                return found.ValueOr(default);
            }

            // Not found yet. We create a new list.
            FixedList64<int> newList = new FixedList64<int>();
            this.actionMap.AddOrSet(effect, newList);
            return newList;
        }

        public readonly GoapAction GetAction(int index) {
            if (index < 0 || index >= this.actionsCount) {
                // Out of bounds
                throw new Exception($"Action {index} is out of bounds of length {this.actionsCount} for {this.name.ToString()} goap domain!");
            }

            return this.actions[index];
        }

        /// <summary>
        /// Returns all actions whose effect is the specified effect
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public readonly ValueTypeOption<FixedList64<int>> GetActionIndices(in Condition effect) {
            return this.actionMap.Find(effect);
        }
    }
}