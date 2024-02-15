using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

namespace Common {
    [BurstCompile]
    public static class NativeContainerUtils {
        [BurstCompile]
        public static void InsertionSort<TItem, TComparer>(ref NativeList<TItem> list, TComparer comparer)
            where TItem : unmanaged
            where TComparer : unmanaged, IComparer<TItem> {
            for (int i = 1; i < list.Length; i++) {
                for (int previousIndex = i - 1; previousIndex >= 0; --previousIndex) {
                    int currentIndex = previousIndex + 1;
                    TItem currentItem = list[currentIndex];
                    TItem previousItem = list[previousIndex];
                    int comparison = comparer.Compare(previousItem, currentItem);
                    if (comparison > 0) {
                        // Previous is larger than current. Swap.
                        list[previousIndex] = currentItem;
                        list[currentIndex] = previousItem;
                    } else {
                        break;
                    }
                }
            }
        }
    }
}