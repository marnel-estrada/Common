using System;

using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// This will be the values that will be maintained as DynamicBuffer associated with the
    /// Option entity.
    /// </summary>
    [InternalBufferCapacity(16)]
    public readonly struct UtilityValue : IBufferElementData, IEquatable<UtilityValue> {
        public readonly int rank;
        public readonly float bonus;
        public readonly float multiplier;

        /// <summary>
        /// Note here that default bonus and multiplier is one instead of zero.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="bonus"></param>
        /// <param name="multiplier"></param>
        public UtilityValue(int rank, float bonus = 1.0f, float multiplier = 1.0f) {
            this.rank = rank;
            this.bonus = bonus;
            this.multiplier = multiplier;
        }

        public float Weight {
            get {
                return this.bonus * this.multiplier;
            }
        }

        public bool Equals(UtilityValue other) {
            return this.rank == other.rank && this.bonus.TolerantEquals(other.bonus) && 
                this.multiplier.TolerantEquals(other.multiplier);
        }

        public override bool Equals(object? obj) {
            return obj is UtilityValue other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = this.rank;
                hashCode = (hashCode * 397) ^ this.bonus.GetHashCode();
                hashCode = (hashCode * 397) ^ this.multiplier.GetHashCode();

                return hashCode;
            }
        }

        public static bool operator ==(UtilityValue left, UtilityValue right) {
            return left.Equals(right);
        }

        public static bool operator !=(UtilityValue left, UtilityValue right) {
            return !left.Equals(right);
        }
    }
}