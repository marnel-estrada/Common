using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    public struct DeallocateNativeArrayJob<T> : IJob where T : struct {
        [DeallocateOnJobCompletion]
        public NativeArray<T> array;
        
        public void Execute() {
            // Does nothing. It just deallocates the specified NativeArray. This is usually used
            // to end a chain of jobs that uses the array along the way.
        }
    }
}