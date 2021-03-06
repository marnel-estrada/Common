using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    public static class MultithreadedSort {
        public static JobHandle Sort<T>(NativeArray<T> array, JobHandle parentHandle = new JobHandle())
            where T : unmanaged, IComparable<T> {
            if (array.Length == 0) {
                // Nothing to sort
                return parentHandle;
            }
            
            int processorCount = math.max(1, SystemInfo.processorCount);
            int threshold = math.max(4, array.Length / processorCount);
            return MergeSort(array, parentHandle, new SortRange(0, array.Length - 1), threshold);
        }

        private static JobHandle MergeSort<T>(NativeArray<T> array, JobHandle parentHandle, SortRange range, int threshold) where T : unmanaged, IComparable<T> {
            if (range.Length <= threshold) {
                // Use quicksort when sub-array length is less than or equal to the threshold
                return new QuicksortJob<T>() {
                    array = array,
                    left = range.min,
                    right = range.max
                }.Schedule(parentHandle);
            }

            int middle = range.Middle;

            SortRange left = new SortRange(range.min, middle);
            JobHandle leftHandle = MergeSort(array, parentHandle, left, threshold);

            SortRange right = new SortRange(middle + 1, range.max);
            JobHandle rightHandle = MergeSort(array, parentHandle, right, threshold);
            
            JobHandle combined = JobHandle.CombineDependencies(leftHandle, rightHandle);
            
            return new Merge<T>() {
                array = array,
                first = left,
                second = right
            }.Schedule(combined);
        }

        public readonly struct SortRange {
            public readonly int min;
            public readonly int max;

            public SortRange(int min, int max) {
                this.min = min;
                this.max = max;
            }

            public int Length {
                get {
                    return this.max - this.min + 1;
                }
            }

            public int Middle {
                get {
                    return (this.min + this.max) >> 1; // divide 2
                }
            }
        }

        [BurstCompile]
        public struct Merge<T> : IJob where T : unmanaged, IComparable<T> {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> array;
            
            public SortRange first;
            public SortRange second;
            
            public void Execute() {
                int firstIndex = this.first.min;
                int secondIndex = this.second.min;
                int resultIndex = this.first.min;

                // Copy first
                NativeArray<T> copy = new NativeArray<T>(this.second.max - this.first.min + 1, Allocator.Temp);
                for (int i = this.first.min; i <= this.second.max; ++i) {
                    int copyIndex = i - this.first.min; 
                    copy[copyIndex] = this.array[i];
                }

                while (firstIndex <= this.first.max || secondIndex <= this.second.max) {
                    if (firstIndex <= this.first.max && secondIndex <= this.second.max) {
                        // both subranges still have elements
                        T firstValue = copy[firstIndex - this.first.min];
                        T secondValue = copy[secondIndex - this.first.min];

                        if (firstValue.CompareTo(secondValue) < 0) {
                            // first value is lesser
                            this.array[resultIndex] = firstValue;
                            ++firstIndex;
                            ++resultIndex;
                        } else {
                            this.array[resultIndex] = secondValue;
                            ++secondIndex;
                            ++resultIndex;
                        }
                    } else if (firstIndex <= this.first.max) {
                        // Only the first range has remaining elements
                        T firstValue = copy[firstIndex - this.first.min];
                        this.array[resultIndex] = firstValue;
                        ++firstIndex;
                        ++resultIndex;
                    } else if (secondIndex <= this.second.max) {
                        // Only the second range has remaining elements
                        T secondValue = copy[secondIndex - this.first.min];
                        this.array[resultIndex] = secondValue;
                        ++secondIndex;
                        ++resultIndex;
                    }
                }

                copy.Dispose();
            }
        }

        [BurstCompile]
        public struct QuicksortJob<T> : IJob where T : unmanaged, IComparable<T> {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> array;

            public int left;
            public int right;

            public void Execute() {
                if (this.array.Length > 0) {
                    Quicksort(this.left, this.right);
                }
            }

            private void Quicksort(int left, int right) {
                int i = left;
                int j = right;
                T pivot = this.array[(left + right) / 2];

                while (i <= j) {
                    // Lesser
                    while (this.array[i].CompareTo(pivot) < 0) {
                        ++i;
                    }

                    // Greater
                    while (this.array[j].CompareTo(pivot) > 0) {
                        --j;
                    }

                    if (i <= j) {
                        // Swap
                        T temp = this.array[i];
                        this.array[i] = this.array[j];
                        this.array[j] = temp;

                        ++i;
                        --j;
                    }
                }

                // Recurse
                if (left < j) {
                    Quicksort(left, j);
                }

                if (i < right) {
                    Quicksort(i, right);
                }
            }
        }
    }
}