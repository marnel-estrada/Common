using System;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is a wrapper to a ConditionId such that we don't have to use FixedString64 in
    /// the components. This way, we save archetype space.
    /// </summary>
    public readonly struct ConditionId : IEquatable<ConditionId> {
        public readonly int hashCode;

        public ConditionId(int hashCode) {
            this.hashCode = hashCode;
        }

        public bool Equals(ConditionId other) {
            return this.hashCode == other.hashCode;
        }

        public override bool Equals(object obj) {
            return obj is ConditionId other && Equals(other);
        }

        public override int GetHashCode() {
            return this.hashCode;
        }

        public static bool operator ==(ConditionId left, ConditionId right) {
            return left.Equals(right);
        }

        public static bool operator !=(ConditionId left, ConditionId right) {
            return !left.Equals(right);
        }
    }
}