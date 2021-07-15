using System;

namespace CommonEcs {
    /// <summary>
    /// A bit array that wraps an int which has 4 bytes (32 bits so 32 slots)
    /// </summary>
    [Serializable]
    public struct BitArray32 : IEquatable<BitArray32> {
        public int internalValue;

        private const int MAX = 32;

        public bool this[int index] {
            get {
                if (index < 0 || index >= MAX) {
                    throw new Exception("Invalid index");
                }
                
                return (this.internalValue & (1 << index)) != 0;
            }

            set {
                if (index < 0 || index >= 32) {
                    throw new Exception("Invalid index");
                }
                
                if (value) {
                    // Turn on bit
                    this.internalValue |= 1 << index;
                } else {
                    // Turn off bit
                    int mask = ~(1 << index);
                    this.internalValue &= mask;
                }
            }
        }

        public int InternalValue {
            get {
                return this.internalValue;
            }
        }

        public void Clear() {
            this.internalValue = 0;
        }

        public bool Equals(BitArray32 other) {
            return this.internalValue == other.internalValue;
        }

        public override bool Equals(object? obj) {
            return obj is BitArray32 other && Equals(other);
        }

        public override int GetHashCode() {
            return this.internalValue;
        }

        public static bool operator ==(BitArray32 left, BitArray32 right) {
            return left.Equals(right);
        }

        public static bool operator !=(BitArray32 left, BitArray32 right) {
            return !left.Equals(right);
        }
    }
}