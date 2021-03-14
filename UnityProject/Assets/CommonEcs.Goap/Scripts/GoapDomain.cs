using Unity.Collections;

namespace CommonEcs.Goap {
    public struct GoapDomain {
        // This is the list of actions
        public FixedList4096<GoapPlanningAction> actions;
        
        // The list of integers mapped to the condition are indices to actions
        public FixedHashMap<Condition, FixedList32<int>> actionMap;

        public void AddAction(in GoapPlanningAction action) {
            int actionIndex = this.actions.Length;
            this.actions.Add(action);
            
            // Add to action map
            FixedList32<int> indexList = ResolveFixedList(action.effect);
            indexList.Add(actionIndex);
            
            // Update map
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

        public GoapPlanningAction GetAction(int index) {
            return this.actions[index];
        }

        public ValueTypeOption<FixedList32<int>> GetActionIndices(Condition condition) {
            return this.actionMap.Find(condition);
        }
    }
}