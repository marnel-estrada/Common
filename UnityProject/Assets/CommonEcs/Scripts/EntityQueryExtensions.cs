using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    public static class EntityQueryExtensions {
        /// <summary>
        /// An easier to use wrapper for EntityQuery.CalculateBaseEntityIndexArrayAsync().
        /// </summary>
        /// <param name="query"></param>
        /// <param name="allocator"></param>
        /// <param name="inputDeps"></param>
        /// <param name="chunkBaseEntityIndices"></param>
        /// <returns></returns>
        public static JobHandle GetBaseEntityIndexArrayAsync(this ref EntityQuery query, Allocator allocator, 
            JobHandle inputDeps, out NativeArray<int> chunkBaseEntityIndices) {
            chunkBaseEntityIndices = query.CalculateBaseEntityIndexArrayAsync(
                allocator, inputDeps, out JobHandle chunkBaseIndicesHandle);
            
            inputDeps = JobHandle.CombineDependencies(inputDeps, chunkBaseIndicesHandle);
            return inputDeps;
        }
    }
}