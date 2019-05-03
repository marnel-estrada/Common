using System;

using Unity.Entities;

namespace CommonEcs {
    [InternalBufferCapacity(10)]
    public readonly struct EcsHashMapEntry<K, V> : IBufferElementData 
        where K : struct, IEquatable<K>
        where V : struct {
        public readonly K key;
        public readonly int hashCode;
        
        public readonly V value;

        public EcsHashMapEntry(K key, V value) {
            this.key = key;
            this.hashCode = key.GetHashCode();
            this.value = value;
        }
    }
}