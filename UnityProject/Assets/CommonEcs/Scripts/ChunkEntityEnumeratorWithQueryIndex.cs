using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// We combined both ChunkEntityEnumerator and EntityIndexAide so that there are no mistakes in
    /// getting a queryIndex from EntityIndexAide for cases when entity iteration continues on the next
    /// entity. EntityIndexAide.NextEntityIndexInQuery() should be called for every index in ChunkEntityEnumerator.
    /// </summary>
    public struct ChunkEntityEnumeratorWithQueryIndex {
        private ChunkEntityEnumerator entityEnumerator;
        private EntityIndexAide indexAide; 

        public ChunkEntityEnumeratorWithQueryIndex(bool useEnabledMask, v128 chunkEnabledMask, int chunkEntityCount, ref NativeArray<int> chunkBaseEntityIndices, int unfilteredChunkIndex) {
            this.entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunkEntityCount);
            this.indexAide = new EntityIndexAide(ref chunkBaseEntityIndices, unfilteredChunkIndex);
        }

        public bool NextEntity(out int i, out int queryIndex) {
            bool hasNext = this.entityEnumerator.NextEntityIndex(out i);
            if (hasNext) {
                // There are still entities. We call NextEntityIndexInQuery() as well.
                queryIndex = this.indexAide.NextEntityIndexInQuery();
                return hasNext;
            }
            
            // No more entities
            queryIndex = -1;
            return false;
        }
    }
}