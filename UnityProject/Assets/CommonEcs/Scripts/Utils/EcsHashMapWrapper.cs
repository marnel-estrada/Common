using System;
using System.Collections;
using System.Collections.Generic;

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
    public struct EcsHashMapWrapper<K, V> where K : struct, IEquatable<K> where V : struct {
        // The entity that points to the buckets
        private readonly Entity hashMapEntity;

        private ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps;

        // The contents of each bucket is an entity that points to the array of elements.
        private readonly Maybe<BufferFromEntity<EntityBufferElement>> allBuckets;

        // The entities here have DynamicBuffer that contains the HashMap values
        private readonly Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists;

        private readonly ValueTypeOption<EntityManager> entityManager;

        public EcsHashMapWrapper(Entity hashMapEntity, ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps,
            BufferFromEntity<EntityBufferElement> allBuckets,
            BufferFromEntity<EcsHashMapEntry<K, V>> allEntryLists) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = new Maybe<BufferFromEntity<EntityBufferElement>>(allBuckets);
            this.allEntryLists = new Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>>(allEntryLists);

            // Note here that this is None. We can't use static NONE as it is not allowed in Burst.
            this.entityManager = new ValueTypeOption<EntityManager>();
        }

        // This is another version that uses EntityManager instead of BufferFromEntity
        public EcsHashMapWrapper(Entity hashMapEntity, ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps,
            EntityManager entityManager) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = Maybe<BufferFromEntity<EntityBufferElement>>.Nothing;
            this.allEntryLists = Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>>.Nothing;

            // Note here that this is Some. We can't use static NONE as it is not allowed in Burst.
            this.entityManager = new ValueTypeOption<EntityManager>(entityManager);
        }

        public void AddOrSet(K key, V newValue) {
            int hashCode = key.GetHashCode();
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveEntryList(hashCode);

            // Search for similar key. Throw exception if we find an entry with similar key.
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
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveEntryList(hashCode);

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

        public Maybe<V> Find(K key) {
            int hashCode = key.GetHashCode();
            DynamicBuffer<EcsHashMapEntry<K, V>> valueList = ResolveEntryList(hashCode);

            // Search for the value with the same key
            for (int i = 0; i < valueList.Length; ++i) {
                EcsHashMapEntry<K, V> entry = valueList[i];
                if (entry.hashCode == hashCode) {
                    // We found it
                    return new Maybe<V>(entry.value);
                }
            }

            // Not found
            return Maybe<V>.Nothing;
        }

        private DynamicBuffer<EcsHashMapEntry<K, V>> ResolveEntryList(int hashCode) {
            int bucketIndex = hashCode % EcsHashMap<K, V>.BUCKET_COUNT;
            return this.entityManager.Match<ResolveEntryListMatcher, DynamicBuffer<EcsHashMapEntry<K, V>>>(
                new ResolveEntryListMatcher(this.hashMapEntity, bucketIndex, this.allBuckets, this.allEntryLists));
        }

        private readonly struct ResolveEntryListMatcher : 
            IFuncOptionMatcher<EntityManager, DynamicBuffer<EcsHashMapEntry<K, V>>> {
            private readonly Entity hashMapEntity;
            private readonly int bucketIndex;
            private readonly Maybe<BufferFromEntity<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists;

            public ResolveEntryListMatcher(Entity hashMapEntity, int bucketIndex, 
                Maybe<BufferFromEntity<EntityBufferElement>> allBuckets, 
                Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists) {
                this.hashMapEntity = hashMapEntity;
                this.bucketIndex = bucketIndex;
                this.allBuckets = allBuckets;
                this.allEntryLists = allEntryLists;
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnSome(EntityManager entityManager) {
                // Use EntityManager
                DynamicBuffer<EntityBufferElement> buckets =
                    entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
                Entity entryListEntity = buckets[bucketIndex].entity;

                return entityManager.GetBuffer<EcsHashMapEntry<K, V>>(entryListEntity);
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnNone() {
                // EntityManager was not specified. Use BufferFromEntity
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
            private readonly Maybe<BufferFromEntity<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists;

            public GetEnumeratorMatcher(Entity hashMapEntity, Maybe<BufferFromEntity<EntityBufferElement>> allBuckets, Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists) {
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
            private readonly Maybe<BufferFromEntity<EntityBufferElement>> allBuckets;
            private readonly Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists;

            public ClearMatcher(Entity hashMapEntity, Maybe<BufferFromEntity<EntityBufferElement>> allBuckets, Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists) {
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