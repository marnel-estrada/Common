using Unity.Collections;

namespace CommonEcs.Goap {
    public struct GoapDomain {
        public readonly FixedString64 name;
        
        // This is the list of actions
        public FixedList4096<GoapAction> actions;
        
        // The list of integers mapped to the condition are indices to actions
        public FixedHashMap<Condition, FixedList32<int>> actionMap;

        public GoapDomain(FixedString64 name) : this() {
            this.name = name;
        }

        public void AddAction(in GoapAction action) {
            int actionIndex = this.actions.Length;
            this.actions.Add(action);
            
            // Add to action map
            FixedList32<int> indexList = ResolveFixedList(action.effect);
            indexList.Add(actionIndex);
            
            // We need to do insertion sort here because we need to sort the actions by cost
            // Bubble down the added action until its cost is in the right place
            for (int i = indexList.Length - 1; i > 0; --i) {
                float currentCost = this.actions[indexList[i]].cost;
                float previousCost = this.actions[indexList[i - 1]].cost;

                if (Comparison.TolerantGreaterThanOrEquals(currentCost, previousCost)) {
                    // This means that the action at i is already in its correct place
                    break;
                }
                
                // At this point, this means that the action at i has lesser cost than the action
                // at i - 1
                // We swap action indices
                int temp = indexList[i];
                indexList[i] = indexList[i - 1];
                indexList[i - 1] = temp;
            }
            
            // Update map (because we are using structs)
            this.actionMap.AddOrSet(action.effect, indexList);
        }

        private FixedList32<int> ResolveFixedList(in Condition effect) {
            ValueTypeOption<FixedList32<int>> found = this.actionMap.Find(effect);
            if (found.IsSome) {
                return found.ValueOr(default);
            }
            
            // Not found yet. We create a new list.
            FixedList32<int> newList = new FixedList32<int>();
            this.actionMap.AddOrSet(effect, newList);
            return newList;
        }

        public readonly GoapAction GetAction(int index) {
            return this.actions[index];
        }

        /// <summary>
        /// Returns all actions whose effect is the specified effect
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public readonly ValueTypeOption<FixedList32<int>> GetActionIndices(in Condition effect) {
            return this.actionMap.Find(effect);
        }
    }
}