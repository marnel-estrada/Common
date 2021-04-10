using System.Collections.Generic;

using Common;

namespace GoapBrain {
    /// <summary>
    /// Handles the set of actions that may alter the value of the condition
    /// </summary>
    public class ConditionActionsSet {
        private readonly ConditionId conditionId;
        private readonly Dictionary<bool, List<GoapAction>> actionsMap = new Dictionary<bool, List<GoapAction>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditionId"></param>
        public ConditionActionsSet(ConditionId conditionId) {
            this.conditionId = conditionId;
        }

        /// <summary>
        /// Adds an action to the set
        /// </summary>
        /// <param name="action"></param>
        public void Add(GoapAction action) {
            Condition effect = FindEffect(action);
            ResolveList(effect.Value).Add(action);
        }

        private Condition FindEffect(GoapAction action) {
            int effectCount = action.EffectCount;
            for (int i = 0; i < effectCount; ++i) {
                Condition effect = action.GetEffectAt(i);
                if (effect.Id == this.conditionId) {
                    return effect;
                }
            }

            Assertion.IsTrue(false, "Can't find effect for " + this.conditionId);
            return null;
        }

        private List<GoapAction> ResolveList(bool value) {
            if(this.actionsMap.TryGetValue(value, out List<GoapAction> list)) {
                // The list was already made
                return list;
            }

            // Lazy initialize the list
            list = new List<GoapAction>();
            this.actionsMap[value] = list;

            return list;
        }
        
        /// <summary>
        /// Returns the action count for the specified condition value
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public int GetActionCount(bool conditionValue) {
            if (this.actionsMap.TryGetValue(conditionValue, out List<GoapAction> list)) {
                // The list is available
                return list.Count;
            }

            // If no list was found, then action count is zero
            return 0;
        }

        /// <summary>
        /// Returns the action that results to the specified condition at the specified index
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public GoapAction GetAction(bool conditionValue, int index) {
            if (this.actionsMap.TryGetValue(conditionValue, out List<GoapAction> list)) {
                // The list is available
                return list[index];
            }

            Assertion.IsTrue(false, $"Can't resolve action ({conditionValue}, {index})");
            return null;
        }

        /// <summary>
        /// Sorts the actions from least cost
        /// </summary>
        public void Sort() {
            foreach(KeyValuePair<bool, List<GoapAction>> entry in this.actionsMap) {
                entry.Value.Sort(AscendingCostComparison);
            }
        }

        private static int AscendingCostComparison(GoapAction a, GoapAction b) {
            if(a.Cost < b.Cost) {
                return -1;
            }

            if(a.Cost > b.Cost) {
                return 1;
            }

            // Equal
            return 0;
        }
    }
}
