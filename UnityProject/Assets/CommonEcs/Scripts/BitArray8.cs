using System;

namespace CommonEcs {
    public struct BitArray8 : IEquatable<BitArray8> {
        private byte internalValue;

        private const int MAX = 8;

        public bool this[int index] {
            get {
                if (index < 0 || index >= MAX) {
                    throw new Exception("Invalid index");
                }
                
                return (this.internalValue & (1 << index)) != 0;
            }

            set {
                if (index < 0 || index >= MAX) {
                    throw new Exception("Invalid index");
                }
                
                if (value) {
                    // Turn on bit
                    this.internalValue |= (byte)(1 << index);
                } else {
                    // Turn off bit
                    int mask = ~(1 << index);
                    this.internalValue = (byte)(this.internalValue & mask);
                }
            }
        }

        public byte InternalValue {
            get {
                return this.internalValue;
            }
        }

        public void Clear() {
            this.internalValue = 0;
        }

        public bool Equals(BitArray8 other) {
            return this.internalValue == other.internalValue;
        }

        public override bool Equals(object? obj) {
            return obj is BitArray8 other && Equals(other);
        }

        public override int GetHashCode() {
            return this.internalValue.GetHashCode();
        }

        public static bool operator ==(BitArray8 left, BitArray8 right) {
            return left.Equals(right);
        }

        public static bool operator !=(BitArray8 left, BitArray8 right) {
            return !left.Equals(right);
        }
    }
}