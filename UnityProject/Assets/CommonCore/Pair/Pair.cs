namespace Common {
    /// <summary>
    /// Pair implemented as a truct
    /// </summary>
    public readonly struct Pair<T1, T2> {
        public readonly T1 first;
        public readonly T2 second;

        public Pair(T1 first, T2 second) {
            this.first = first;
            this.second = second;
        }
    }
}