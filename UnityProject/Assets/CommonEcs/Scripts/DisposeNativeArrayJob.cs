using Unity.Collections;
using Unity.Jobs;

namespace DefaultNamespace {
    public readonly struct DisposeNativeArrayJob<T> : IJob where T : unmanaged {
        [DeallocateOnJobCompletion]
        public readonly NativeArray<T> array;

        public DisposeNativeArrayJob(NativeArray<T> array) {
            this.array = array;
        }

        public void Execute() {
        }
    }
}