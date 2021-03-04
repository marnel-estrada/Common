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

        public ValueTypeOption<bool> GetValue(int hashcode) {
            for (int i = 0; i < this.count; ++i) {
                if (this.hashes[i] == hashcode) {
                    // Hashcode found
                    return ValueTypeOption<bool>.Some(this.values[i]);
                }
            }
            
            // Not found
            return ValueTypeOption<bool>.None;
        }

        public bool Contains(int hashcode) {
            for (int i = 0; i < this.count; ++i) {
                if (this.hashes[i] == hashcode) {
                    // Hashcode found
                    return true;
                }
            }

            return false;
        }

        public void SetValueAtIndex(int index, bool value) {
            this.values[index] = value;
        }

        /// <summary>
        /// Returns whether or not an item was removed or not
        /// </summary>
        /// <param name="hashcode"></param>
        /// <returns></returns>
        public bool Remove(int hashcode) {
            for (int i = 0; i < this.count; ++i) {
                if (this.hashes[i] == hashcode) {
                    // Item found
                    // To remove, we just shift the next items into the removed slot
                    ShiftItemsFrom(i);
                    --this.count;

                    return true;
                }
            }

            return false;
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

        public int GetHashCodeAtIndex(int index) {
            if (index < 0 || index >= MAX) {
                // We can't use string interpolation here as this will be invoked in Burst
                throw new Exception(string.Format("Invalid index: {0}", index));
            }
            
            return this.hashes[index];
        }

        public void Clear() {
            // We no longer need to clear the values here since the slot will be replaced
            // upon adding a new entry
            this.count = 0;
        }
    }
}