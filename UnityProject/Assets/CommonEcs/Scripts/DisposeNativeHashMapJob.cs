using System;

using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    public struct DisposeNativeHashMapJob<K, V> : IJob 
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        public NativeParallelHashMap<K, V> map;
        
        public void Execute() {
            this.map.Dispose();
        }
    }
}