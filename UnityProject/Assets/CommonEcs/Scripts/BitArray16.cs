using System;

namespace CommonEcs {
    public struct BitArray16 : IEquatable<BitArray16> {
        private char internalValue;

        private const int MAX = 16;

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
                    this.internalValue |= (char)(1 << index);
                } else {
                    // Turn off bit
                    int mask = ~(1 << index);
                    this.internalValue = (char)(this.internalValue & mask);
                }
            }
        }

        public char InternalValue {
            get {
                return this.internalValue;
            }
        }

        public void Clear() {
            this.internalValue = (char)0;
        }

        public bool Equals(BitArray16 other) {
            return this.internalValue == other.internalValue;
        }

        public override bool Equals(object? obj) {
            return obj is BitArray16 other && Equals(other);
        }

        public override int GetHashCode() {
            return this.internalValue.GetHashCode();
        }

        public static bool operator ==(BitArray16 left, BitArray16 right) {
            return left.Equals(right);
        }

        public static bool operator !=(BitArray16 left, BitArray16 right) {
            return !left.Equals(right);
        }
    }
}