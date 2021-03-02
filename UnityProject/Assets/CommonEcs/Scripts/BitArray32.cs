using System;

namespace CommonEcs {
    /// <summary>
    /// A bit array that wraps an int which has 4 bytes (32 bits so 32 slots)
    /// </summary>
    public struct BitArray32 {
        private int bitArray;

        private const int MAX = 32;

        public bool this[int index] {
            get {
                if (index < 0 || index >= MAX) {
                    throw new Exception("Invalid index");
                }
                
                return (this.bitArray & (1 << index)) != 0;
            }

            set {
                if (index < 0 || index >= 32) {
                    throw new Exception("Invalid index");
                }
                
                if (value) {
                    // Turn on bit
                    this.bitArray |= 1 << index;
                } else {
                    // Turn off bit
                    int mask = ~(1 << index);
                    this.bitArray &= mask;
                }
            }
        }

        public void Clear() {
            this.bitArray = 0;
        }
    }
}