using System;

namespace CommonEcs {
    public readonly struct LinearHashMapEntry<K, V>
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        public readonly V value;
        public readonly K key;
        public readonly int hashCode;
        public readonly bool hasValue; // This is to discriminate entries with no value

        public LinearHashMapEntry(K key, int keyHashCode, V value) {
            this.key = key;
            this.hashCode = keyHashCode;
            this.value = value;
            this.hasValue = true;
        }

        public static LinearHashMapEntry<K, V> Nothing {
            get {
                return default;
            }
        }
    }
}