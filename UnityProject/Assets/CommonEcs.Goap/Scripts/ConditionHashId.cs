using System;

using Unity.Collections;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is a wrapper to a ConditionId such that we don't have to use FixedString64Bytes in
    /// the components. This way, we save archetype space.
    ///
    /// We renamed this to ConditionHashId since GoapBrain package also has ConditionId.
    /// </summary>
    public readonly struct ConditionHashId : IEquatable<ConditionHashId> {
        public readonly int hashCode;
        
        public ConditionHashId(FixedString64Bytes stringId) : this(stringId.GetHashCode()) {
        }

        public ConditionHashId(int hashCode) {
            this.hashCode = hashCode;
        }

        public bool Equals(ConditionHashId other) {
            return this.hashCode == other.hashCode;
        }

        public override bool Equals(object? obj) {
            return obj is ConditionHashId other && Equals(other);
        }

        public override int GetHashCode() {
            return this.hashCode;
        }

        public static bool operator ==(ConditionHashId left, ConditionHashId right) {
            return left.Equals(right);
        }

        public static bool operator !=(ConditionHashId left, ConditionHashId right) {
            return !left.Equals(right);
        }
    }
}