using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    public static class MultithreadedSort {
        public static JobHandle Sort<T>(NativeArray<T> array, JobHandle parentHandle = new())
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

            SortRange left = new(range.min, middle);
            JobHandle leftHandle = MergeSort(array, parentHandle, left, threshold);

            SortRange right = new(middle + 1, range.max);
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

            public int Length => this.max - this.min + 1;

            public int Middle => (this.min + this.max) >> 1; // divide 2
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
                NativeArray<T> copy = new(this.second.max - this.first.min + 1, Allocator.Temp);
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
                        (this.array[i], this.array[j]) = (this.array[j], this.array[i]);

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
        
        public static JobHandle SortWithComparer<T, U>(ref NativeArray<T> array, ref U comparer, 
            int startIndex, int endIndex, JobHandle parentHandle = new())
            where T : unmanaged
            where U : unmanaged, IComparer<T> {
            if (array.Length == 0) {
                // Nothing to sort
                return parentHandle;
            }
            
            int processorCount = math.max(1, SystemInfo.processorCount);
            int threshold = math.max(4, array.Length / processorCount);
            return MergeSortWithComparer(ref array, ref comparer, parentHandle, new SortRange(startIndex, endIndex), 
                threshold);
        }
        
        private static JobHandle MergeSortWithComparer<T, U>(ref NativeArray<T> array, ref U comparer, 
            JobHandle parentHandle, SortRange range, int threshold) 
            where T : unmanaged 
            where U : unmanaged, IComparer<T> {
            if (range.Length <= threshold) {
                // Use quicksort when sub-array length is less than or equal to the threshold
                return new QuicksortJobWithComparer<T, U>() {
                    array = array,
                    comparer = comparer,
                    left = range.min,
                    right = range.max
                }.Schedule(parentHandle);
            }

            int middle = range.Middle;

            SortRange left = new(range.min, middle);
            JobHandle leftHandle = MergeSortWithComparer(ref array, ref comparer, parentHandle, left, threshold);

            SortRange right = new(middle + 1, range.max);
            JobHandle rightHandle = MergeSortWithComparer(ref array, ref comparer, parentHandle, right, threshold);
            
            JobHandle combined = JobHandle.CombineDependencies(leftHandle, rightHandle);
            
            return new MergeWithComparer<T, U>() {
                array = array,
                comparer = comparer,
                first = left,
                second = right
            }.Schedule(combined);
        }
        
        [BurstCompile]
        public struct MergeWithComparer<T, U> : IJob 
            where T : unmanaged 
            where U : unmanaged, IComparer<T> {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> array;

            [ReadOnly]
            public U comparer;
            
            public SortRange first;
            public SortRange second;
            
            public void Execute() {
                int firstIndex = this.first.min;
                int secondIndex = this.second.min;
                int resultIndex = this.first.min;

                // Copy first
                NativeArray<T> copy = new(this.second.max - this.first.min + 1, Allocator.Temp);
                for (int i = this.first.min; i <= this.second.max; ++i) {
                    int copyIndex = i - this.first.min; 
                    copy[copyIndex] = this.array[i];
                }

                while (firstIndex <= this.first.max || secondIndex <= this.second.max) {
                    if (firstIndex <= this.first.max && secondIndex <= this.second.max) {
                        // both subranges still have elements
                        T firstValue = copy[firstIndex - this.first.min];
                        T secondValue = copy[secondIndex - this.first.min];

                        if (this.comparer.Compare(firstValue, secondValue) < 0) {
                            // first value is lesser
                            this.array[resultIndex] = firstValue;
                            ++firstIndex;
                        } else {
                            this.array[resultIndex] = secondValue;
                            ++secondIndex;
                        }

                        ++resultIndex;
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
        public struct QuicksortJobWithComparer<T, U> : IJob 
            where T : unmanaged 
            where U : unmanaged, IComparer<T> {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> array;

            [ReadOnly]
            public U comparer;

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
                    while (this.comparer.Compare(this.array[i], pivot) < 0) {
                        ++i;
                    }

                    // Greater
                    while (this.comparer.Compare(this.array[j], pivot) > 0) {
                        --j;
                    }

                    if (i > j) {
                        continue;
                    }

                    // Swap
                    (this.array[i], this.array[j]) = (this.array[j], this.array[i]);

                    ++i;
                    --j;
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