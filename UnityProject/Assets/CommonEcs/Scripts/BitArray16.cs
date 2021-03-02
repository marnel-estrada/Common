using System;

namespace CommonEcs {
    public struct BitArray16 {
        private char bitArray;

        private const int MAX = 16;

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
                    this.bitArray |= (char)(1 << index);
                } else {
                    // Turn off bit
                    int mask = ~(1 << index);
                    this.bitArray = (char)(this.bitArray & mask);
                }
            }
        }

        public void Clear() {
            this.bitArray = (char)0;
        }
    }
}