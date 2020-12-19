using System;

using CommonEcs;

namespace GoapBrainEcs {
    public readonly struct Condition : IEquatable<Condition> {
        public readonly ushort id;
        public readonly ByteBool value;

        public Condition(ushort id, bool value) {
            this.id = id;
            this.value = value;
        }

        public static bool operator ==(Condition a, Condition b) {
            return a.id == b.id && a.value.Value == b.value.Value;
        }

        public static bool operator !=(Condition a, Condition b) {
            return !(a == b);
        }

        public bool Equals(Condition other) {
            return this.id == other.id && this.value.Equals(other.value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is Condition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.id.GetHashCode() * 397) ^ this.value.GetHashCode();
            }
        }
    }
}