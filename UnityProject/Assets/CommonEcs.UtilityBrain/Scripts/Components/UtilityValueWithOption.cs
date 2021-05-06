using System;

using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [InternalBufferCapacity(16)]
    public readonly struct UtilityValueWithOption : IBufferElementData, IEquatable<UtilityValueWithOption> {
        public readonly UtilityValue value;
        
        // The option entity where this value was derived from
        public readonly Entity optionEntity;

        public UtilityValueWithOption(Entity optionEntity, UtilityValue value) {
            this.optionEntity = optionEntity;
            this.value = value;
        }

        public bool Equals(UtilityValueWithOption other) {
            return this.value.Equals(other.value) && this.optionEntity.Equals(other.optionEntity);
        }

        public override bool Equals(object? obj) {
            return obj is UtilityValueWithOption other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.value.GetHashCode() * 397) ^ this.optionEntity.GetHashCode();
            }
        }

        public static bool operator ==(UtilityValueWithOption left, UtilityValueWithOption right) {
            return left.Equals(right);
        }

        public static bool operator !=(UtilityValueWithOption left, UtilityValueWithOption right) {
            return !left.Equals(right);
        }
    }
}