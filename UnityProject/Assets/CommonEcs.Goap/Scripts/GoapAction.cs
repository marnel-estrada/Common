using System;

using Unity.Collections;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is an action that is used in planning. This is not the actual action that
    /// will be executed.
    /// </summary>
    public struct GoapAction : IEquatable<GoapAction> {
        public ConditionList10 preconditions;
        public readonly Condition effect;
        
        // We can't use FixedString here to save space when being used in FixedHashMap
        public readonly int id;
        
        public readonly float cost;

        // This is used to identify if execution of atom actions is already done
        public readonly int atomActionsCount;

        public readonly FixedString64 name;

        public GoapAction(int id, float cost, int atomActionsCount, in Condition effect) : this() {
            this.id = id;
            this.cost = cost;
            this.atomActionsCount = atomActionsCount;
            this.preconditions = new ConditionList10();
            this.effect = effect;
        }
        
        /// <summary>
        /// Constructor with default atomActionsCount
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cost"></param>
        /// <param name="effect"></param>
        public GoapAction(int id, float cost, in Condition effect) : this(id, cost, 1, effect) {
        }
        
        public GoapAction(FixedString64 name, float cost, int atomActionsCount, in Condition effect) :
            this(name.GetHashCode(), cost, atomActionsCount, effect) {
            this.name = name;
        }

        public void AddPrecondition(Condition condition) {
            this.preconditions.Add(condition);
        }

        public bool Equals(GoapAction other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is GoapAction other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(GoapAction left, GoapAction right) {
            return left.Equals(right);
        }

        public static bool operator !=(GoapAction left, GoapAction right) {
            return !left.Equals(right);
        }
    }
}