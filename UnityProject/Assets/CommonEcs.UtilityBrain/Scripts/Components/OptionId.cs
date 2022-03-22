using System;

using Unity.Collections;

namespace CommonEcs.UtilityBrain {
    public struct OptionId : IEquatable<OptionId> {
        private readonly int value;

        public OptionId(int value) {
            this.value = value;
        }

        public OptionId(in FixedString32Bytes textId) : this(textId.GetHashCode()) {
        }
        
        public OptionId(in FixedString64Bytes textId) : this(textId.GetHashCode()) {
        }

        public bool Equals(OptionId other) {
            return this.value == other.value;
        }

        public override bool Equals(object? obj) {
            return obj is OptionId other && Equals(other);
        }

        public override int GetHashCode() {
            return this.value;
        }

        public static bool operator ==(OptionId left, OptionId right) {
            return left.Equals(right);
        }

        public static bool operator !=(OptionId left, OptionId right) {
            return !left.Equals(right);
        }
    }
}