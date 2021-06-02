using Unity.Entities;

namespace CommonEcs.Goap {
    public interface IConditionResolverProcess<T> where T : unmanaged, IConditionResolverComponent {
        /// <summary>
        /// Routines before chunk iteration. Calling ArchetypeChunk.GetNativeArray()
        /// can be called here.
        /// </summary>
        /// <param name="batchInChunk"></param>
        /// <param name="batchIndex"></param>
        void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex);
        
        bool IsMet(in Entity agentEntity, ref T resolverComponent, int indexOfFirstEntityInQuery, int iterIndex);
    }
}