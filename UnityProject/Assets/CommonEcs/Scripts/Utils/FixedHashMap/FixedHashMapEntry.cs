namespace Common {
    public readonly struct FixedHashMapEntry<V>
        where V : unmanaged {
        public readonly V value;
        public readonly int hashCode;

        public FixedHashMapEntry(int hashCode, V value) {
            this.hashCode = hashCode;
            this.value = value;
        }
    }
}