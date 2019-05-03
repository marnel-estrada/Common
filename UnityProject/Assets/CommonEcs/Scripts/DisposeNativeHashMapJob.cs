using System;

using Unity.Collections;
using Unity.Jobs;

namespace CommonEcs {
    public struct DisposeNativeHashMapJob<K, V> : IJob 
        where K : struct, IEquatable<K>
        where V : struct {
        public NativeHashMap<K, V> map;
        
        public void Execute() {
            this.map.Dispose();
        }
    }
}