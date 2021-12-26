using System;

namespace CommonEcs {
    public readonly struct LinearHashMapEntry<K, V>
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        public readonly V value;
        public readonly K key;
        
        public readonly bool hasValue; // This is to discriminate entries with no value

        public LinearHashMapEntry(K key, V value) {
            this.key = key;
            this.value = value;
            this.hasValue = true;
        }

        public int HashCode {
            get {
                return this.key.GetHashCode();
            }
        }

        public static LinearHashMapEntry<K, V> Nothing {
            get {
                return default;
            }
        }
    }
}