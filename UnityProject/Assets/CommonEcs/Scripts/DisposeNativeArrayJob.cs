using Unity.Collections;
using Unity.Jobs;

namespace DefaultNamespace {
    public readonly struct DisposeNativeArrayJob<T> : IJob where T : struct {
        [DeallocateOnJobCompletion]
        public readonly NativeArray<T> array;

        public DisposeNativeArrayJob(NativeArray<T> array) {
            this.array = array;
        }

        public void Execute() {
        }
    }
}