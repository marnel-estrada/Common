using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    public struct GoapDomain {
        // This is the list of actions
        public BlobArray<GoapPlanningAction> actions;
        
        // The list of integers mapped to the condition are indices to actions
        public FixedHashMap<Condition, FixedList32<int>> actionMap;

        public GoapPlanningAction GetAction(int index) {
            return this.actions[index];
        }

        public ValueTypeOption<FixedList32<int>> GetActionIndices(Condition condition) {
            return this.actionMap.Find(condition);
        }
    }
}