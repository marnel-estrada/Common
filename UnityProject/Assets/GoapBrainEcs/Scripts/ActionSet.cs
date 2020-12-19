using System.Collections.Generic;

namespace GoapBrainEcs {
    /// <summary>
    /// Class that collects actions that resolves to a specified condition
    /// </summary>
    public class ActionSet {
        private readonly ushort conditionId;
        private readonly List<GoapAction> trueActions = new List<GoapAction>(1);
        private readonly List<GoapAction> falseActions = new List<GoapAction>(1);

        public ActionSet(ushort conditionId) {
            this.conditionId = conditionId;
        }

        /// <summary>
        /// Adds an action to the set
        /// </summary>
        /// <param name="action"></param>
        public void Add(GoapAction action) {
            ResolveList(action.effect.value).Add(action);
        }

        private List<GoapAction> ResolveList(bool value) {
            return value ? this.trueActions : this.falseActions;
        }

        /// <summary>
        /// Returns the number of actions for the specified value
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public int GetActionCount(bool conditionValue) {
            return ResolveList(conditionValue).Count;
        }

        /// <summary>
        /// Returns the action for the specified value and at the specified index
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public GoapAction GetAction(bool conditionValue, int index) {
            // This will throw error if index is out of bounds
            return ResolveList(conditionValue)[index];
        }

        private static System.Comparison<GoapAction> COMPARISON;

        /// <summary>
        /// Sorts the actions according to cost
        /// </summary>
        public void Sort() {
            if (COMPARISON == null) {
                COMPARISON = AscendingCostComparison;
            }
            
            this.trueActions.Sort(COMPARISON);
            this.falseActions.Sort(COMPARISON);
        }
        
        private static int AscendingCostComparison(GoapAction a, GoapAction b) {
            if(a.cost < b.cost) {
                return -1;
            }

            if(a.cost > b.cost) {
                return 1;
            }

            // Equal
            return 0;
        }

        public IReadOnlyList<GoapAction> GetActions(bool value) {
            return value ? this.trueActions : this.falseActions;
        }
    }
}