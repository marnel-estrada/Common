using System;

namespace CommonEcs {
    public readonly struct FixedHashMapEntry<K, V>
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        public readonly K key;
        public readonly V value;
        public readonly int hashCode;

        public FixedHashMapEntry(K key, V value) {
            this.key = key;
            this.hashCode = this.key.GetHashCode();
            this.value = value;
        }
    }
}