using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Common interface that will compute a utility.
    /// This is like the IConsideration implementation in OOP UtilityBrain.
    /// </summary>
    public interface IConsiderationProcess<T> where T : unmanaged, IConsiderationComponent {
        /// <summary>
        /// Routines before chunk iteration. Calling ArchetypeChunk.GetNativeArray()
        /// can be called here.
        /// </summary>
        /// <param name="batchInChunk"></param>
        /// <param name="batchIndex"></param>
        void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex);
        
        UtilityValue ComputeUtility(in Entity agentEntity, in T filterComponent, int indexOfFirstEntityInQuery, int iterIndex);
    }
}