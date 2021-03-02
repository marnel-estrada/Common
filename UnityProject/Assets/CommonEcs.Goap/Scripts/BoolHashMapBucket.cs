using System;

namespace CommonEcs.Goap {
    /// <summary>
    /// This is a bucket to a BoolHashMap
    /// </summary>
    public struct BoolHashMapBucket {
        private IntStackArray16 hashes;
        private int count;
        private BitArray16 values;

        public const int MAX = 16;

        public void Add(int hashcode, bool value) {
            if (hashcode < 0) {
                throw new Exception("Hashcode can't be negative");
            }

            if (this.count >= MAX) {
                throw new Exception("Exceeded max items");
            }

            int index = this.count;
            this.hashes[index] = hashcode;
            this.values[index] = value;
            ++this.count;
        }

        public ValueTypeOption<bool> Get(int hashcode) {
            for (int i = 0; i < this.count; ++i) {
                if (this.hashes[i] == hashcode) {
                    // Hashcode found
                    return ValueTypeOption<bool>.Some(this.values[i]);
                }
            }
            
            // Not found
            return ValueTypeOption<bool>.None;
        }

        public void Remove(int hashcode) {
            for (int i = 0; i < this.count; ++i) {
                if (this.hashes[i] == hashcode) {
                    // Item found
                    // To remove, we just shift the next items into the removed slot
                    ShiftItemsFrom(i);
                    --this.count;

                    break;
                }
            }
        }

        private void ShiftItemsFrom(int index) {
            for (int i = index + 1; i < this.count; ++i) {
                this.hashes[i - 1] = this.hashes[i];
                this.values[i - 1] = this.values[i];
            }
        }
        
        public int Count {
            get {
                return this.count;
            }
        }
    }
}