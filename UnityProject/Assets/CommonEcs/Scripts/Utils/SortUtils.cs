using System.Collections.Generic;
using Unity.Collections;

namespace CommonEcs {
    public static class SortUtils {
        /// <summary>
        /// We might want to call some sorting methods inside jobs where the parameters may only be resolved when the
        /// job executes. Like the values of left or right might be coming from a list where the contents is only
        /// resolved by a previous job. 
        /// </summary>
        public static void Quicksort<T, U>(ref NativeArray<T> array, ref U comparer, int left, int right) 
            where T : unmanaged
            where U : unmanaged, IComparer<T> {
            int i = left;
            int j = right;
            T pivot = array[(left + right) / 2];

            while (i <= j) {
                // Lesser
                while (comparer.Compare(array[i], pivot) < 0) {
                    ++i;
                }

                // Greater
                while (comparer.Compare(array[j], pivot) > 0) {
                    --j;
                }

                if (i > j) {
                    continue;
                }

                // Swap
                (array[i], array[j]) = (array[j], array[i]);

                ++i;
                --j;
            }

            // Recurse
            if (left < j) {
                Quicksort(ref array, ref comparer, left, j);
            }

            if (i < right) {
                Quicksort(ref array, ref comparer,i, right);
            }
        }
    }
}