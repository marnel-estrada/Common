using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public struct BlobHashMap<K, V> 
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged {
        [StructLayout(LayoutKind.Sequential)]
        public struct Entry {
            public K key;
            public V value;
            public int next;
        }

        internal BlobArray<int> buckets;
        internal BlobArray<Entry> data;
        internal int halfPv;

        public int Count {
            get {
                return this.data.Length;
            }
        }

        public ref V Get(in K key) {
            if (this.buckets.Length == -1) {
                throw new Exception();
            }

            if (this.halfPv <= 0 || this.Count <= 5) {
                for (int i = 0, c = this.Count; i < c; ++i) {
                    ref Entry entry = ref this.data[i];

                    if (entry.key.Equals(key)) {
                        return ref entry.value;
                    }
                }
            } else {
                int bucketIndex = key.GetHashCode() % this.halfPv;

                bucketIndex = this.buckets[bucketIndex + this.halfPv];

                while (bucketIndex != -1) {
                    ref Entry entry = ref this.data[bucketIndex];
                    ref K entryKey = ref entry.key;
                    if (entryKey.Equals(key)) {
                        return ref entry.value;
                    }

                    bucketIndex = entry.next;
                }
            }

            throw new KeyNotFoundException();
        }

        public bool ContainsKey(in K key) {
            if (this.halfPv <= 0 || this.Count <= 5) {
                for (int i = 0, c = this.Count; i < c; ++i) {
                    ref Entry entry = ref this.data[i];

                    if (entry.key.Equals(key)) {
                        return true;
                    }
                }
            } else {
                int bucketIndex = key.GetHashCode() % this.halfPv;

                bucketIndex = this.buckets[bucketIndex + this.halfPv];

                while (bucketIndex != -1) {
                    ref Entry entry = ref this.data[bucketIndex];

                    if (entry.key.Equals(key)) {
                        return true;
                    }

                    bucketIndex = entry.next;
                }
            }

            return false;
        }

        public bool TryGetValue(in K key, out V value) {
            if (this.halfPv <= 0 || this.Count <= 5) {
                for (int i = 0, c = this.Count; i < c; ++i) {
                    ref Entry entry = ref this.data[i];

                    if (entry.key.Equals(key)) {
                        value = entry.value;

                        return true;
                    }
                }
            } else {
                int bucketIndex = this.buckets[key.GetHashCode() % this.halfPv + this.halfPv];

                while (bucketIndex != -1) {
                    ref Entry entry = ref this.data[bucketIndex];

                    if (entry.key.Equals(key)) {
                        value = entry.value;

                        return true;
                    }

                    bucketIndex = entry.next;
                }
            }

            value = default;

            return false;
        }

        public NativeArray<K> GetKeys(Allocator allocator) {
            int length = this.data.Length;

            NativeArray<K> array = new NativeArray<K>(length, allocator, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < length; ++i) {
                ref Entry entry = ref this.data[i];
                array[i] = entry.key;
            }

            return array;
        }
    }
}