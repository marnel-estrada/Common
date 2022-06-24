using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A random but weighted selection amongst items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct WeightedSelection<T> where T : unmanaged, IEquatable<T> {
        private NativeList<T> items;
        private NativeList<WeightEntry> entries;

        private NativeParallelHashMap<T, int> itemToIndexMap;
        
        public WeightedSelection(int initialCapacity, Allocator allocator) {
            this.items = new NativeList<T>(initialCapacity, allocator);
            this.entries = new NativeList<WeightEntry>(initialCapacity, allocator);
            this.itemToIndexMap = new NativeParallelHashMap<T, int>(initialCapacity, allocator);
        }

        public void Add(in T item, uint weight) {
            DotsAssert.IsTrue(weight > 0); // Weight can't be zero
            
            // Add to mapping from item to index
            this.itemToIndexMap.Add(item, this.items.Length);
            
            this.items.Add(item);

            int entriesLength = this.entries.Length;
            
            // If there are no entries yet, then it's the starting item. Start is zero.
            // Otherwise, we take the previous item and the start is the next number of its end.
            uint start = entriesLength == 0 ? 0 : this.entries[entriesLength - 1].end + 1;
            uint end = entriesLength == 0 ? weight - 1 : this.entries[entriesLength - 1].end + weight;
            
            this.entries.Add(new WeightEntry(weight, start, end));
            
            DotsAssert.IsTrue(this.items.Length == this.entries.Length);
            DotsAssert.IsTrue(this.items.Length == this.itemToIndexMap.Count());
        }

        public readonly T Select(ref Unity.Mathematics.Random random) {
            DotsAssert.IsTrue(!this.entries.IsEmpty); // Should have entries
        
            uint lastItemEnd = this.entries[this.entries.Length - 1].end;
            
            // We add 1 here because random selection of integers is max exclusive
            uint randomValue = random.NextUInt(0, lastItemEnd + 1);
            
            // Use binary search to search the entry where the value falls into
            int minIndex = 0;
            int maxIndex = this.entries.Length - 1;
            int iterationCount = 0; // used for getting out of stack overflow

            while (iterationCount <= this.entries.Length) {
                if (minIndex == maxIndex) {
                    // This means that there's no more items in between them
                    // So return the item
                    return this.items[minIndex];
                }

                int middle = (minIndex + maxIndex) / 2;
                WeightEntry entry = this.entries[middle];

                if (entry.IsWithinRange(randomValue)) {
                    // Item found
                    return this.items[middle];
                }

                // Middle is not the item. We move search to left or right.
                if (randomValue < entry.start) {
                    // This means that item is to the left of the mid entry
                    // We search the left half
                    maxIndex = middle - 1;
                } else {
                    // If not to the left of mid entry, it can only be to the right
                    // We search the right half
                    minIndex = middle + 1;
                }

                ++iterationCount;
            }

            throw new Exception("Unable so select an item. Something is wrong.");
        }

        /// <summary>
        /// Updates the weight of the specified item such its selection range is altered (increased
        /// or reduced)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newWeight"></param>
        public void UpdateWeight(in T item, uint newWeight) {
            int itemIndex = this.itemToIndexMap[item];
            
            // Update the weight of the item
            UpdateWeight(itemIndex, newWeight);

            // Adjust the ranges of the items to the right
            AdjustWeights(itemIndex + 1);
        }

        private void AdjustWeights(int startIndex) {
            for (int i = startIndex; i < this.entries.Length; ++i) {
                // Updates the ranges using the same weight
                UpdateWeight(i, this.entries[i].weight);
            }
        }

        private void UpdateWeight(int index, uint newWeight) {
            uint start = index == 0 ? 0 : this.entries[index - 1].end + 1;
            uint end = index == 0 ? newWeight - 1 : this.entries[index - 1].end + newWeight;
            this.entries[index] = new WeightEntry(newWeight, start, end);
        }

        public void Dispose() {
            this.items.Dispose();
            this.entries.Dispose();
            this.itemToIndexMap.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) {
            inputDeps = this.items.Dispose(inputDeps);
            inputDeps = this.entries.Dispose(inputDeps);
            inputDeps = this.itemToIndexMap.Dispose(inputDeps);

            return inputDeps;
        }

        [BurstDiscard]
        public void PrintItems() {
            for (int i = 0; i < this.items.Length; ++i) {
                WeightEntry entry = this.entries[i];
                Debug.Log($"{this.items[i].ToString()}: {entry.weight} [{entry.start} - {entry.end}]");
            }
        }
    }
}