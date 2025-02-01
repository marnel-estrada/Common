using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [BurstCompile]
    public struct ComputeConsiderationsJob<TFilter, TProcessor> : IJobChunk 
        where TFilter : unmanaged, IConsiderationComponent
        where TProcessor : unmanaged, IConsiderationProcess<TFilter> {
        [ReadOnly]
        public NativeArray<int> chunkBaseEntityIndices;

        public ComponentTypeHandle<Consideration> considerationType;

        public ComponentTypeHandle<TFilter> filterType;

        public bool filterHasArray;
        public TProcessor processor;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            NativeArray<Consideration> considerations = chunk.GetNativeArray(ref this.considerationType);
            
            NativeArray<TFilter> filters = this.filterHasArray ? chunk.GetNativeArray(ref this.filterType) : default;
            TFilter defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
            
            this.processor.BeforeChunkIteration(chunk);

            ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
            while (enumerator.NextEntity(out int i, out int queryIndex)) {
                Consideration consideration = considerations[i];
                if (!consideration.shouldExecute) {
                    // Not time to execute yet
                    continue;
                }

                // Compute the utility
                if (this.filterHasArray) {
                    // Use filter component if it has data
                    TFilter filter = filters[i];
                    consideration.value = this.processor.ComputeUtility(consideration.agentEntity, filter, i, queryIndex);
                } else {
                    // Filter has no component. Just use default data.
                    consideration.value = this.processor.ComputeUtility(consideration.agentEntity, defaultFilter, i, queryIndex);
                }
                
                // Modify
                considerations[i] = consideration;
            }
        }
    }
}