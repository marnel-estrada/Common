namespace CommonEcs {
    public readonly struct PoolEntry<T> where T : struct {
        public readonly T instance;
        public readonly int index;

        public PoolEntry(T instance, int index) {
            this.instance = instance;
            this.index = index;
        }
    }
}