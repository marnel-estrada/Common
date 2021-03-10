using System;
using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    public static class BlobHashMapExtensions {
        private static bool IsPrime(int n) {
            if (n < 2) {
                return false;
            }

            for (int i = n - 1; i > 1; i--) {
                if (n % i == 0) {
                    return false;
                }
            }

            return true;
        }

        private static void InternalAllocateHashMap<K, V>(ref this BlobBuilder builder,
            ref BlobHashMap<K, V> hashMap, Dictionary<K, V> dictionary) 
            where K : unmanaged, IEquatable<K>
            where V : unmanaged {
            int count = dictionary.Count;

            if (count == 0) {
                return;
            }

            int pV = 2;

            if (count > 2) {
                for (int i = count - 1; i >= 0; --i) {
                    if (!IsPrime(i)) {
                        continue;
                    }

                    pV = i;
                    break;
                }
            }

            BlobBuilderArray<int> bucketArray = builder.Allocate(ref hashMap.buckets, pV);
            BlobBuilderArray<BlobHashMap<K, V>.Entry> dataArray = builder.Allocate(ref hashMap.data, count);
            int halfPv = pV / 2;
            hashMap.halfPv = halfPv;

            for (int i = 0; i < pV; ++i) {
                bucketArray[i] = -1;
            }

            int entryIndex = 0;
            foreach (KeyValuePair<K, V> pair in dictionary) {
                int bucketIndex = pair.Key.GetHashCode() % halfPv + halfPv;

                dataArray[entryIndex] = new BlobHashMap<K, V>.Entry {
                    next = -1
                };

                ref BlobHashMap<K, V>.Entry blobEntry = ref dataArray[entryIndex];
                blobEntry.key = pair.Key;
                blobEntry.value = pair.Value;

                if (bucketArray[bucketIndex] == -1) {
                    bucketArray[bucketIndex] = entryIndex;
                } else {
                    int tempIndex = bucketArray[bucketIndex];

                    while (dataArray[tempIndex].next != -1) {
                        tempIndex = dataArray[tempIndex].next;
                    }

                    ref BlobHashMap<K, V>.Entry v = ref dataArray[tempIndex];
                    v.next = entryIndex;
                }

                ++entryIndex;
            }
        }

        public static void AllocateHashMap<K, V>(ref this BlobBuilder builder,
            ref BlobHashMap<K, V> hashMap, Dictionary<K, V> value) 
            where K : unmanaged, IEquatable<K>
            where V : unmanaged {
            InternalAllocateHashMap(ref builder, ref hashMap, value);
        }
    }
}