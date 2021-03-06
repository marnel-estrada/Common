using UnityEngine.Assertions;

namespace GoapBrainEcs {
    public struct GoapAction {
        public readonly ushort id;
        public readonly float cost;
        public ConditionList10 preconditions;
        public readonly Condition effect;

        public GoapAction(ushort id, float cost, Condition effect) {
            this.id = id;
            this.cost = cost;
            this.preconditions = new ConditionList10();
            this.effect = effect;
        }

        public void AddPrecondition(Condition condition) {
            // Should not contain the specified condition yet
            Assert.IsFalse(ContainsPrecondition(condition.id));
            this.preconditions.Add(condition);
        }

        public void AddPrecondition(ushort conditionId, bool value) {
            AddPrecondition(new Condition(conditionId, value));
        }

        public bool ContainsPrecondition(ushort conditionId) {
            for (int i = 0; i < this.preconditions.Count; ++i) {
                if (this.preconditions[i].id == conditionId) {
                    return true;
                }
            }

            return false;
        }

        public bool HasPrecondition(Condition condition) {
            for (int i = 0; i < this.preconditions.Count; ++i) {
                if (this.preconditions[i] == condition) {
                    return true;
                }
            }

            return false;
        }
    }
}
