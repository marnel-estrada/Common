using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// This sorts a list without knowing the length of the list
    /// This is useful for cases when a list is being populated by another job prior to this
    /// sorting job.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [BurstCompile]
    public struct QuicksortListJob<T> : IJob where T : unmanaged, IComparable<T> {
        public NativeList<T> list;

        public QuicksortListJob(ref NativeList<T> list) {
            this.list = list;
        }
        
        public void Execute() {
            if (this.list.Length > 0) {
                Quicksort(0, this.list.Length - 1);
            }
        }

        private void Quicksort(int left, int right) {
            while (true) {
                int i = left;
                int j = right;
                T pivot = this.list[(left + right) / 2];

                while (i <= j) {
                    // Lesser
                    while (this.list[i].CompareTo(pivot) < 0) {
                        ++i;
                    }

                    // Greater
                    while (this.list[j].CompareTo(pivot) > 0) {
                        --j;
                    }

                    if (i > j) {
                        continue;
                    }

                    // Swap
                    (this.list[i], this.list[j]) = (this.list[j], this.list[i]);

                    ++i;
                    --j;
                }

                // Recurse
                if (left < j) {
                    Quicksort(left, j);
                }

                if (i < right) {
                    left = i;
                    continue;
                }

                break;
            }
        }
    }
}