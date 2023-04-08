using System;
using System.Collections;
using System.Collections.Generic;

using Common;

using Unity.Entities;

namespace CommonEcs {
    public struct EcsHashMapEnumerator<K, V> : IEnumerator<EcsHashMapEntry<K, V>>
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged {
        private readonly DynamicBuffer<EntityBufferElement> buckets;
        private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;
        private readonly ValueTypeOption<EntityManager> entityManager;

        private int bucketIndex;
        private int entryIndex;

        private DynamicBuffer<EcsHashMapEntry<K, V>> currentEntryList;

        /// <summary>
        /// Constructor with all entry lists
        /// </summary>
        /// <param name="buckets"></param>
        /// <param name="allEntryLists"></param>
        public EcsHashMapEnumerator(DynamicBuffer<EntityBufferElement> buckets,
            Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists) {
            this.buckets = buckets;
            this.allEntryLists = allEntryLists;
            this.bucketIndex = 0;
            this.entryIndex = -1;
            this.entityManager = default;

            // Resolve entry list
            this.currentEntryList = this.allEntryLists.Value[this.buckets[this.bucketIndex].entity];
        }

        public EcsHashMapEnumerator(DynamicBuffer<EntityBufferElement> buckets, EntityManager entityManager) {
            this.buckets = buckets;
            this.allEntryLists = Maybe<BufferLookup<EcsHashMapEntry<K, V>>>.Nothing;
            this.entityManager = ValueTypeOption<EntityManager>.Some(entityManager);

            this.bucketIndex = 0;
            this.entryIndex = -1;

            this.currentEntryList = entityManager.GetBuffer<EcsHashMapEntry<K, V>>(this.buckets[this.bucketIndex].entity);
        }

        public bool MoveNext() {
            ++this.entryIndex;
            while (this.entryIndex >= this.currentEntryList.Length) {
                // Move to next bucket
                ++this.bucketIndex;
                if (this.bucketIndex >= this.buckets.Length) {
                    // End of iteration
                    return false;
                }

                this.currentEntryList = ResolveEntryList(this.buckets[this.bucketIndex].entity);
                
                // We set to zero here instead of -1 because the next call to Current is to use
                // the entryIndex == 0
                this.entryIndex = 0; 
            }

            return true;
        }

        public void Reset() {
            this.bucketIndex = 0;
            this.entryIndex = 0;
        }

        public EcsHashMapEntry<K, V> Current {
            get {
                return this.currentEntryList[this.entryIndex];
            }
        }

        object IEnumerator.Current {
            get {
                return this.Current;
            }
        }

        public void Dispose() {
        }

        private DynamicBuffer<EcsHashMapEntry<K, V>> ResolveEntryList(Entity entryListEntity) {
            return this.entityManager.Match<ResolveEntryListMatcher, DynamicBuffer<EcsHashMapEntry<K, V>>>(
                new ResolveEntryListMatcher(entryListEntity, this.allEntryLists));
        }

        private readonly struct ResolveEntryListMatcher : IFuncOptionMatcher<EntityManager, DynamicBuffer<EcsHashMapEntry<K, V>>> {
            private readonly Entity entryListEntity;
            private readonly Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists;

            public ResolveEntryListMatcher(Entity entryListEntity, Maybe<BufferLookup<EcsHashMapEntry<K, V>>> allEntryLists) {
                this.entryListEntity = entryListEntity;
                this.allEntryLists = allEntryLists;
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnSome(EntityManager entityManager) {
                return entityManager.GetBuffer<EcsHashMapEntry<K, V>>(this.entryListEntity);
            }

            public DynamicBuffer<EcsHashMapEntry<K, V>> OnNone() {
                return this.allEntryLists.Value[this.entryListEntity];
            }
        }
    }
}