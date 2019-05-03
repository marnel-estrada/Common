using System;
using System.Collections;
using System.Collections.Generic;

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
    public struct EcsHashMapWrapper<K, V> : IEnumerable<EcsHashMapEntry<K, V>>
        where K : struct, IEquatable<K> where V : struct {
        // The entity that points to the buckets
        private readonly Entity hashMapEntity;

        private ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps;

        // The contents of each bucket is an entity that points to the array of elements.
        private readonly Maybe<BufferFromEntity<EntityBufferElement>> allBuckets;

        // The entities here have DynamicBuffer that contains the HashMap values
        private readonly Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>> allEntryLists;

        private readonly EntityManager entityManager;

        public EcsHashMapWrapper(Entity hashMapEntity, ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps,
            BufferFromEntity<EntityBufferElement> allBuckets,
            BufferFromEntity<EcsHashMapEntry<K, V>> allEntryLists) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = new Maybe<BufferFromEntity<EntityBufferElement>>(allBuckets);
            this.allEntryLists = new Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>>(allEntryLists);

            this.entityManager = null;
        }

        // This is another version that uses EntityManager instead of BufferFromEntity
        public EcsHashMapWrapper(Entity hashMapEntity, ComponentDataFromEntity<EcsHashMap<K, V>> allHashMaps,
            EntityManager entityManager) {
            this.hashMapEntity = hashMapEntity;
            this.allHashMaps = allHashMaps;

            this.allBuckets = Maybe<BufferFromEntity<EntityBufferElement>>.Nothing;
            this.allEntryLists = Maybe<BufferFromEntity<EcsHashMapEntry<K, V>>>.Nothing;

            this.entityManager = entityManager;
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

            if (this.entityManager == null) {
                // EntityManager was not specified. Use BufferFromEntity
                DynamicBuffer<EntityBufferElement> buckets = this.allBuckets.Value[this.hashMapEntity];
                Entity entryListEntity = buckets[bucketIndex].entity;

                return this.allEntryLists.Value[entryListEntity];
            }

            // Use EntityManager
            {
                DynamicBuffer<EntityBufferElement> buckets =
                    this.entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
                Entity entryListEntity = buckets[bucketIndex].entity;

                return this.entityManager.GetBuffer<EcsHashMapEntry<K, V>>(entryListEntity);
            }
        }

        public int Count {
            get {
                return this.allHashMaps[this.hashMapEntity].count;
            }
        }

        public IEnumerator<EcsHashMapEntry<K, V>> GetEnumerator() {
            DynamicBuffer<EntityBufferElement> buckets =
                this.entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);

            if (this.entityManager == null) {
                return new EcsHashMapEnumerator<K, V>(buckets, this.allEntryLists);
            }
            
            return new EcsHashMapEnumerator<K, V>(buckets, this.entityManager);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Clear() {
            DynamicBuffer<EntityBufferElement> buckets =
                this.entityManager.GetBuffer<EntityBufferElement>(this.hashMapEntity);
            for (int i = 0; i < buckets.Length; ++i) {
                Entity entryListEntity = buckets[i].entity;
                DynamicBuffer<EcsHashMapEntry<K, V>> entryList = this.entityManager?.GetBuffer<EcsHashMapEntry<K, V>>(entryListEntity) ?? this.allEntryLists.Value[entryListEntity];
                entryList.Clear();
            }
            
            // Update the count
            EcsHashMap<K, V> hashMap = this.allHashMaps[this.hashMapEntity];
            hashMap.count = 0;
            this.allHashMaps[this.hashMapEntity] = hashMap; // Modify
        }
    }
}