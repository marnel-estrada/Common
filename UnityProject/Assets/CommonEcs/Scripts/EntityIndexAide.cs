using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// This is utility for getting the entity index when doing chunk iteration
    /// We did this because it's so cumbersome to write for the moment
    /// This is based from https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/upgrade-guide.html#jobs-that-need-firstentityindex
    /// </summary>
    public struct EntityIndexAide {
        private readonly int baseEntityIndex;
        private int validEntitiesInChunk;

        public EntityIndexAide(ref NativeArray<int> chunkBaseEntityIndices, int unfilteredChunkIndex) : this() {
            this.baseEntityIndex = chunkBaseEntityIndices[unfilteredChunkIndex];
        }

        public int NextEntityIndexInQuery() {
            int entityIndexInQuery = this.baseEntityIndex + this.validEntitiesInChunk;
            ++this.validEntitiesInChunk;

            return entityIndexInQuery;
        }
    }
}