namespace CommonEcs {
    /// <summary>
    /// A bit array that wraps an int which has 4 bytes (32 bits so 32 slots)
    /// </summary>
    public struct BitArrayInt {
        private int bitArray;

        public bool this[int index] {
            get {
                return (this.bitArray & (1 << index)) != 0;
            }

            set {
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