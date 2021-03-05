using System;

namespace CommonEcs.Goap {
    public readonly struct Condition : IEquatable<Condition> {
        public readonly ConditionId id;
        public readonly bool value;

        public Condition(ConditionId id, bool value) {
            this.id = id;
            this.value = value;
        }

        public bool Equals(Condition other) {
            return this.id.Equals(other.id) && this.value == other.value;
        }

        public override bool Equals(object obj) {
            return obj is Condition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.id.GetHashCode() * 397) ^ this.value.GetHashCode();
            }
        }

        public static bool operator ==(Condition left, Condition right) {
            return left.Equals(right);
        }

        public static bool operator !=(Condition left, Condition right) {
            return !left.Equals(right);
        }
    }
}