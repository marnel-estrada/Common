using CommonEcs.Goap;

using Unity.Collections;
using Unity.Entities;

namespace GoalSelector {
    public readonly struct GoalCondition : IComponentData {
        public readonly Condition goal;

        public GoalCondition(in FixedString64Bytes conditionName, bool value) {
            this.goal = new Condition(conditionName, value);
        }
    }
}