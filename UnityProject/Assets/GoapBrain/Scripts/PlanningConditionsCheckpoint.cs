using System.Collections.Generic;

using Common;

namespace GoapBrain {
    class PlanningConditionsCheckpoint {
        private int index;
        private readonly SimpleList<ConditionChange> changes = new SimpleList<ConditionChange>();

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        public void Init(int index) {
            this.index = index;
            this.changes.Clear();
        }

        public int Index {
            get {
                return index;
            }
        }

        /// <summary>
        /// Adds the specified condition change
        /// </summary>
        /// <param name="change"></param>
        public void Add(ConditionId id, bool previousValue, bool updatedValue) {
            this.changes.Add(new ConditionChange(id, previousValue, updatedValue));
        }

        /// <summary>
        /// Applys the changes in this checkpoint to the specified values map
        /// </summary>
        /// <param name="valuesMap"></param>
        public void Revert(Dictionary<ConditionId, bool> valuesMap) {
            for(int i = 0; i < this.changes.Count; ++i) {
                ConditionChange change = this.changes[i];
                valuesMap[change.id] = change.previousValue;
            }
        }
    }
}
