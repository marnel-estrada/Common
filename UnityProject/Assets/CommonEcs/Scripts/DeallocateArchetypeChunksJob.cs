namespace CommonEcs {
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    [BurstCompile]
    public struct DeallocateArchetypeChunksJob : IJob {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> chunks;

        public void Execute() {
            // Does nothing. It just deallocates the chunks.
        }
    }
}
