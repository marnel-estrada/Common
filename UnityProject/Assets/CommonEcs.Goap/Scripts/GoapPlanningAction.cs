using System;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is an action that is used in planning. This is not the actual action that
    /// will be executed.
    /// </summary>
    public struct GoapPlanningAction : IEquatable<GoapPlanningAction> {
        public ConditionList10 preconditions;
        public readonly Condition effect;
        
        // We can't use FixedString here to save space when being used in FixedHashMap
        public readonly int id;
        public readonly float cost;

        public GoapPlanningAction(int id, float cost, Condition effect) {
            this.id = id;
            this.cost = cost;
            this.preconditions = new ConditionList10();
            this.effect = effect;
        }

        public void AddPrecondition(Condition condition) {
            this.preconditions.Add(condition);
        }

        public bool Equals(GoapPlanningAction other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is GoapPlanningAction other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(GoapPlanningAction left, GoapPlanningAction right) {
            return left.Equals(right);
        }

        public static bool operator !=(GoapPlanningAction left, GoapPlanningAction right) {
            return !left.Equals(right);
        }
    }
}