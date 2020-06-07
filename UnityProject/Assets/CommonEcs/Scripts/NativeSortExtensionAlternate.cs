using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    ///     This is copied from NativeSortExtension where we make the internal jobs public so we
    ///     could mention the concrete uses such that they will be Burst compiled
    /// </summary>
    public static class NativeSortExtensionAlternate {
        private const int k_IntrosortSizeThreshold = 16;

        /// <summary>
        ///     Sorts an array in ascending order.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="array">Array to perform sort.</param>
        /// <param name="length">Number of elements to perform sort.</param>
        private static unsafe void Sort<T>(T* array, int length) where T : unmanaged, IComparable<T> {
            IntroSort<T, DefaultComparer<T>>(array, length, new DefaultComparer<T>());
        }

        /// <summary>
        ///     Sorts a list using a custom comparison function.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="list">List to perform sort.</param>
        /// <param name="comp">
        ///     A comparison function that indicates whether one element in the array is less than, equal to, or
        ///     greater than another element.
        /// </param>
        private static unsafe void Sort<T, U>(this NativeList<T> list, U comp) where T : struct where U : IComparer<T> {
            IntroSort<T, U>(list.GetUnsafePtr(), list.Length, comp);
        }

        /// <summary>
        ///     Sorts a list using a custom comparison function.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="list">List to perform sort.</param>
        /// <param name="comp">
        ///     A comparison function that indicates whether one element in the array is less than, equal to, or
        ///     greater than another element.
        /// </param>
        private static unsafe void Sort<T, U>(this UnsafeList list, U comp) where T : struct where U : IComparer<T> {
            IntroSort<T, U>(list.Ptr, list.Length, comp);
        }

        /// <summary>
        ///     Sorts a slice in ascending order.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="slice">Slice to perform sort.</param>
        private static void Sort<T>(this NativeSlice<T> slice) where T : struct, IComparable<T> {
            slice.Sort(new DefaultComparer<T>());
        }

        /// <summary>
        ///     Sorts a slice using a custom comparison function.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="slice">List to perform sort.</param>
        /// <param name="comp">
        ///     A comparison function that indicates whether one element in the array is less than, equal to, or
        ///     greater than another element.
        /// </param>
        private static unsafe void Sort<T, U>(this NativeSlice<T> slice, U comp)
            where T : struct where U : IComparer<T> {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (slice.Stride != UnsafeUtility.SizeOf<T>()) {
                throw new InvalidOperationException("Sort requires that stride matches the size of the source type");
            }
#endif

            IntroSort<T, U>(slice.GetUnsafePtr(), slice.Length, comp);
        }

        /// -- Internals
        private static unsafe void IntroSort<T, U>(void* array, int length, U comp)
            where T : struct where U : IComparer<T> {
            IntroSort<T, U>(array, 0, length - 1, 2 * CollectionHelper.Log2Floor(length), comp);
        }

        private static unsafe void IntroSort<T, U>(void* array, int lo, int hi, int depth, U comp)
            where T : struct where U : IComparer<T> {
            while (hi > lo) {
                int partitionSize = hi - lo + 1;
                if (partitionSize <= k_IntrosortSizeThreshold) {
                    if (partitionSize == 1) {
                        return;
                    }

                    if (partitionSize == 2) {
                        SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);

                        return;
                    }

                    if (partitionSize == 3) {
                        SwapIfGreaterWithItems<T, U>(array, lo, hi - 1, comp);
                        SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);
                        SwapIfGreaterWithItems<T, U>(array, hi - 1, hi, comp);

                        return;
                    }

                    InsertionSort<T, U>(array, lo, hi, comp);

                    return;
                }

                if (depth == 0) {
                    HeapSort<T, U>(array, lo, hi, comp);

                    return;
                }

                depth--;

                int p = Partition<T, U>(array, lo, hi, comp);
                IntroSort<T, U>(array, p + 1, hi, depth, comp);
                hi = p - 1;
            }
        }

        private static unsafe void InsertionSort<T, U>(void* array, int lo, int hi, U comp)
            where T : struct where U : IComparer<T> {
            int i, j;
            T t;
            for (i = lo; i < hi; i++) {
                j = i;
                t = UnsafeUtility.ReadArrayElement<T>(array, i + 1);
                while (j >= lo && comp.Compare(t, UnsafeUtility.ReadArrayElement<T>(array, j)) < 0) {
                    UnsafeUtility.WriteArrayElement(array, j + 1, UnsafeUtility.ReadArrayElement<T>(array, j));
                    j--;
                }

                UnsafeUtility.WriteArrayElement(array, j + 1, t);
            }
        }

        private static unsafe int Partition<T, U>(void* array, int lo, int hi, U comp)
            where T : struct where U : IComparer<T> {
            int mid = lo + (hi - lo) / 2;
            SwapIfGreaterWithItems<T, U>(array, lo, mid, comp);
            SwapIfGreaterWithItems<T, U>(array, lo, hi, comp);
            SwapIfGreaterWithItems<T, U>(array, mid, hi, comp);

            T pivot = UnsafeUtility.ReadArrayElement<T>(array, mid);
            Swap<T>(array, mid, hi - 1);
            int left = lo, right = hi - 1;

            while (left < right) {
                while (comp.Compare(pivot, UnsafeUtility.ReadArrayElement<T>(array, ++left)) > 0) {
                    ;
                }

                while (comp.Compare(pivot, UnsafeUtility.ReadArrayElement<T>(array, --right)) < 0) {
                    ;
                }

                if (left >= right) {
                    break;
                }

                Swap<T>(array, left, right);
            }

            Swap<T>(array, left, hi - 1);

            return left;
        }

        private static unsafe void HeapSort<T, U>(void* array, int lo, int hi, U comp)
            where T : struct where U : IComparer<T> {
            int n = hi - lo + 1;

            for (int i = n / 2; i >= 1; i--) {
                Heapify<T, U>(array, i, n, lo, comp);
            }

            for (int i = n; i > 1; i--) {
                Swap<T>(array, lo, lo + i - 1);
                Heapify<T, U>(array, 1, i - 1, lo, comp);
            }
        }

        private static unsafe void Heapify<T, U>(void* array, int i, int n, int lo, U comp)
            where T : struct where U : IComparer<T> {
            T val = UnsafeUtility.ReadArrayElement<T>(array, lo + i - 1);
            int child;
            while (i <= n / 2) {
                child = 2 * i;
                if (child < n && comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + child - 1),
                    UnsafeUtility.ReadArrayElement<T>(array, lo + child)) < 0) {
                    child++;
                }

                if (comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lo + child - 1), val) < 0) {
                    break;
                }

                UnsafeUtility.WriteArrayElement(array, lo + i - 1,
                    UnsafeUtility.ReadArrayElement<T>(array, lo + child - 1));
                i = child;
            }

            UnsafeUtility.WriteArrayElement(array, lo + i - 1, val);
        }

        private static unsafe void Swap<T>(void* array, int lhs, int rhs) where T : struct {
            T val = UnsafeUtility.ReadArrayElement<T>(array, lhs);
            UnsafeUtility.WriteArrayElement(array, lhs, UnsafeUtility.ReadArrayElement<T>(array, rhs));
            UnsafeUtility.WriteArrayElement(array, rhs, val);
        }

        private static unsafe void SwapIfGreaterWithItems<T, U>(void* array, int lhs, int rhs, U comp)
            where T : struct where U : IComparer<T> {
            if (lhs != rhs) {
                if (comp.Compare(UnsafeUtility.ReadArrayElement<T>(array, lhs),
                    UnsafeUtility.ReadArrayElement<T>(array, rhs)) > 0) {
                    Swap<T>(array, lhs, rhs);
                }
            }
        }

        /// <summary>
        ///     Sorts an array in ascending order.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="array">Array to perform sort.</param>
        /// <param name="inputDeps">The job handle or handles for any scheduled jobs that use this container.</param>
        /// <returns>
        ///     A new job handle containing the prior handles as well as the handle for the job that sorts
        ///     the container.
        /// </returns>
        public static unsafe JobHandle SortJobAlternate<T>(this NativeArray<T> array, JobHandle inputDeps = new JobHandle())
            where T : unmanaged, IComparable<T> {
            return SortJobAlternate((T*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), array.Length,
                inputDeps);
        }

        /// <summary>
        ///     Sorts an array in ascending order.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="array">Array to perform sort.</param>
        /// <param name="length">Number of elements to perform sort.</param>
        /// <param name="inputDeps">The job handle or handles for any scheduled jobs that use this container.</param>
        /// <returns>
        ///     A new job handle containing the prior handles as well as the handle for the job that sorts
        ///     the container.
        /// </returns>
        public static unsafe JobHandle SortJobAlternate<T>(T* array, int length, JobHandle inputDeps = new JobHandle())
            where T : unmanaged, IComparable<T> {
            if (length == 0) {
                return inputDeps;
            }

            int segmentCount = (length + 1023) / 1024;

            int workerCount = math.max(1, JobsUtility.MaxJobThreadCount);
            int workerSegmentCount = segmentCount / workerCount;
            SegmentSort<T> segmentSortJob = new SegmentSort<T> {
                Data = array, Length = length, SegmentWidth = 1024
            };
            JobHandle segmentSortJobHandle = segmentSortJob.Schedule(segmentCount, workerSegmentCount, inputDeps);
            SegmentSortMerge<T> segmentSortMergeJob = new SegmentSortMerge<T> {
                Data = array, Length = length, SegmentWidth = 1024
            };
            JobHandle segmentSortMergeJobHandle = segmentSortMergeJob.Schedule(segmentSortJobHandle);

            return segmentSortMergeJobHandle;
        }

        private struct DefaultComparer<T> : IComparer<T> where T : IComparable<T> {
            public int Compare(T x, T y) {
                return x.CompareTo(y);
            }
        }

        [BurstCompile]
        public unsafe struct SegmentSort<T> : IJobParallelFor where T : unmanaged, IComparable<T> {
            [NativeDisableUnsafePtrRestriction]
            public T* Data;

            public int Length;
            public int SegmentWidth;

            public void Execute(int index) {
                int startIndex = index * this.SegmentWidth;
                int segmentLength = this.Length - startIndex < this.SegmentWidth ? this.Length - startIndex :
                    this.SegmentWidth;
                Sort(this.Data + startIndex, segmentLength);
            }
        }

        [BurstCompile]
        public unsafe struct SegmentSortMerge<T> : IJob where T : unmanaged, IComparable<T> {
            [NativeDisableUnsafePtrRestriction]
            public T* Data;

            public int Length;
            public int SegmentWidth;

            public void Execute() {
                int segmentCount = (this.Length + (this.SegmentWidth - 1)) / this.SegmentWidth;
                int* segmentIndex = stackalloc int[segmentCount];

                T* resultCopy = (T*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * this.Length, 16, Allocator.Temp);

                for (int sortIndex = 0; sortIndex < this.Length; sortIndex++) {
                    // find next best
                    int bestSegmentIndex = -1;
                    T bestValue = default;

                    for (int i = 0; i < segmentCount; i++) {
                        int startIndex = i * this.SegmentWidth;
                        int offset = segmentIndex[i];
                        int segmentLength = this.Length - startIndex < this.SegmentWidth ? this.Length - startIndex :
                            this.SegmentWidth;
                        if (offset == segmentLength) {
                            continue;
                        }

                        T nextValue = this.Data[startIndex + offset];
                        if (bestSegmentIndex != -1) {
                            if (nextValue.CompareTo(bestValue) > 0) {
                                continue;
                            }
                        }

                        bestValue = nextValue;
                        bestSegmentIndex = i;
                    }

                    segmentIndex[bestSegmentIndex]++;
                    resultCopy[sortIndex] = bestValue;
                }

                UnsafeUtility.MemCpy(this.Data, resultCopy, UnsafeUtility.SizeOf<T>() * this.Length);
            }
        }
    }
}