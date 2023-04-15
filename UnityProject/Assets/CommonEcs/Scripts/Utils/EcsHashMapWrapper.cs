using System;

using Common;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// To create a EcsHashMap, create an entity with an EcsHashMap component
    /// and a buffer of EntityBufferElement (this is for the buckets) 
    /// 
    /// To use this HashMap, instantiate using the constructor supplying all needed parameters
    /// Use as a regular HashMap
    ///
    /// A simple HashMap implementation can be based here: https://dzone.com/articles/custom-hashmap-implementation-in-java
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public struct EcsHashMapWrapper<K, V> 
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged, IEquatable<V> {
        // The entity that points to the buckets
        private readonly Entity hashMapEntity;

        private ComponentLookup<EcsHashMap<K, V>> allHashMaps;

        // The contents of each bucket is an entity that points to the array of elements.
        private readonly Maybe<BufferLookup<EntityBufferElement>> allBuckets;

        // The entities here have DynamicBuffer that contains the HashMap values
        private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;

        private readonly ValueTypeOption<EntityManager> entityManager;

        public EcsHashMapWrapper(Entity hashMapEntity, ComponentLookup<EcsHashMap<K, V>> allHashMaps,
            BufferLookup<EntityBufferElement> allBuckets,
            BufferLookup<EcsHashMapEntry<K, V>> allEntryLists) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = new Maybe<BufferLookup<EntityBufferElement>>(allBuckets);
            this.allEntryLists = new Maybe<BufferLookup<EcsHashMapEntry<K, V>>>(allEntryLists);

            // Note here that this is None. We can't use static NONE as it is not allowed in Burst.
            this.entityManager = new ValueTypeOption<EntityManager>();
        }

        // This is another version that uses EntityManager instead of BufferLookup
        public EcsHashMapWrapper(Entity hashMapEntity, ComponentLookup<EcsHashMap<K, V>> allHashMaps,
            EntityManager entityManager) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = Maybe<BufferLookup<EntityBufferElement>>.Nothing;
            this.allEntryLists = Maybe<BufferLookup<EcsHashMapEntry<K, V>>>.Nothing;

            // Note here that this is Some. We can't use static NONE as it is not allowed in Burst.
            this.entityManager = ValueTypeOption<EntityManager>.Some(entityManager);
        }

        public void AddOrSet(K key, V newValue) {
            int hashCode = key.GetHashCode();
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveBucket(hashCode);

            // Search for similar key. Replace the value if we find an entry with similar key.
            for (int i = 0; i < valueList.Length; ++i) {
                EcsHashMapEntry<K, V> entry = valueList[i];
                if (entry.hashCode == hashCode) {
                    // The HashMap already contains the specified key. We replace it.
                    valueList[i] = new EcsHashMapEntry<K, V>(entry.key, newValue);
                    return;
                }
            }

            valueList.Add(new EcsHashMapEntry<K, V>(key, newValue));

            // Update the count
            EcsHashMap<K, V> hashMap = this.allHashMaps[this.hashMapEntity];
            ++hashMap.count;
            this.allHashMaps[this.hashMapEntity] = hashMap; // Modify 
        }

        public void Remove(K key) {
            int hashCode = key.GetHashCode();
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveBucket(hashCode);

            // Search for the key in the value list and remove that
            for (int i = 0; i < valueList.Length; ++i) {
                EcsHashMapEntry<K, V> entry = valueList[i];
                if (entry.hashCode == hashCode) {
                    valueList.RemoveAt(i);

                    // Update the count
                    EcsHashMap<K, V> hashMap = this.allHashMaps[this.hashMapEntity];
                    --hashMap.count;
                    this.allHashMaps[this.hashMapEntity] = hashMap; // Modify

                    break;
                }
            }
        }

        public ValueTypeOption<V> Find(K key) {
            int hashCode = key.GetHashCode();
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveBucket(hashCode);

            // Search for the value with the same key
            for (int i = 0; i < valueList.Length; ++i) {
                EcsHashMapEntry<K, V> entry = valueList[i];
                if (entry.hashCode == hashCode) {
                    // We found it
                    return ValueTypeOption<V>.Some(entry.value);
                }
            }

            // Not found
            return ValueTypeOption<V>.None;
        }

        private DynamicBuffer<EcsHashMapEntry<K, V>> ResolveBucket(int hashCode) {
            int bucketIndex = FibonacciHash(hashCode);
            return this.entityManager.Match<ResolveEntryListMatcher, DynamicBuffer<EcsHashMapEntry<K, V>>>(
                new ResolveEntryListMatcher(this.hashMapEntity, bucketIndex, this.allBuckets, this.allEntryLists));
        }
        
        // This is taken from https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/
        private static int FibonacciHash(int hash) {
            // This is 2^64 / 1.6180339 (Fibonacci constant)
            const ulong magicNumber = 11400714819323198485;
            
            // We shift 60 bits here as we only need 4 bits (0-15)
            // Note that EcsHashMap.BUCKET_COUNT is 16
            return (int)(((ulong)hash * magicNumber) >> 60);
        }

        private readonly struct ResolveEntryListMatcher : 
            IFuncOptionMatcher<EntityManager, DynamicBuffer<EcsHashMapEntry<K, V>>> {
            private readonly Entity hashMapEntity;
            private readonly int bucketIndex;
            private readonly Maybe<BufferLookup<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;

            public ResolveEntryListMatcher(Entity hashMapEntity, int bucketIndex, 
                Maybe<BufferLookup<EntityBufferElement>> allBuckets, 
                Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists) {
                this.hashMapEntity = hashMapEntity;
                this.bucketIndex = bucketIndex;
                this.allBuckets = allBuckets;
                this.allEntryLists = allEntryLists;
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnSome(EntityManager entityManager) {
                // Use EntityManager
                DynamicBuffer<EntityBufferElement> buckets =
                    entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
                Entity entryListEntity = buckets[this.bucketIndex].entity;

                return entityManager.GetBuffer<EcsHashMapEntry<K, V>>(entryListEntity);
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnNone() {
                // EntityManager was not specified. Use BufferLookup
                DynamicBuffer<EntityBufferElement> buckets = this.allBuckets.Value[this.hashMapEntity];
                Entity entryListEntity = buckets[this.bucketIndex].entity;

                return this.allEntryLists.Value[entryListEntity];
            }
        }

        public int Count {
            get {
                return this.allHashMaps[this.hashMapEntity].count;
            }
        }

        public EcsHashMapEnumerator<K, V> GetEnumerator() {
            return this.entityManager.Match<GetEnumeratorMatcher, EcsHashMapEnumerator<K, V>>(
                new GetEnumeratorMatcher(this.hashMapEntity, this.allBuckets, this.allEntryLists));
        }

        private readonly struct GetEnumeratorMatcher : IFuncOptionMatcher<EntityManager, EcsHashMapEnumerator<K, V>> {
            private readonly Entity hashMapEntity;
            private readonly Maybe<BufferLookup<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;

            public GetEnumeratorMatcher(Entity hashMapEntity, Maybe<BufferLookup<EntityBufferElement>> allBuckets, Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists) {
                this.hashMapEntity = hashMapEntity;
                this.allBuckets = allBuckets;
                this.allEntryLists = allEntryLists;
            }

            public EcsHashMapEnumerator<K, V> OnSome(EntityManager entityManager) {
                // Use EntityManager
                DynamicBuffer<EntityBufferElement> buckets = entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
                return new EcsHashMapEnumerator<K, V>(buckets, entityManager);
            }

            public EcsHashMapEnumerator<K, V> OnNone() {
                DynamicBuffer<EntityBufferElement> buckets = this.allBuckets.Value[this.hashMapEntity];
                return new EcsHashMapEnumerator<K, V>(buckets, this.allEntryLists);
            }
        }

        public void Clear() {
            this.entityManager.Match(new ClearMatcher(this.hashMapEntity, this.allBuckets, this.allEntryLists));
            
            // Update the count
            EcsHashMap<K, V> hashMap = this.allHashMaps[this.hashMapEntity];
            hashMap.count = 0;
            this.allHashMaps[this.hashMapEntity] = hashMap; // Modify
        }

        private readonly struct ClearMatcher : IOptionMatcher<EntityManager> {
            private readonly Entity hashMapEntity;
            private readonly Maybe<BufferLookup<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;

            public ClearMatcher(Entity hashMapEntity, Maybe<BufferLookup<EntityBufferElement>> allBuckets, Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists) {
                this.hashMapEntity = hashMapEntity;
                this.allBuckets = allBuckets;
                this.allEntryLists = allEntryLists;
            }

            public void OnSome(EntityManager entityManager) {
                // Use EntityManager
                DynamicBuffer<EntityBufferElement> buckets =
                    entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
                for (int i = 0; i < buckets.Length; ++i) {
                    Entity entryListEntity = buckets[i].entity;
                    DynamicBuffer<EcsHashMapEntry<K, V>> entryList = entityManager.GetBuffer<EcsHashMapEntry<K, V>>(entryListEntity);
                    entryList.Clear();
                }
            }

            public void OnNone() {
                // No EntityManager
                DynamicBuffer<EntityBufferElement> buckets = this.allBuckets.Value[this.hashMapEntity];
                for (int i = 0; i < buckets.Length; ++i) {
                    Entity entryListEntity = buckets[i].entity;
                    DynamicBuffer<EcsHashMapEntry<K, V>> entryList = this.allEntryLists.Value[entryListEntity];
                    entryList.Clear();
                }
            }
        }
    }
}