namespace Common {
    public readonly struct OptionEqualityMatcher<T> : IFuncOptionMatcher<T, bool> where T : class {
        private readonly Option<T> other;

        public OptionEqualityMatcher(Option<T> other) {
            this.other = other;
        }

        public bool OnSome(T value) {
            return this.other.ReferenceEquals(value);
        }

        public bool OnNone() {
            // Return true if the other is none as well
            return this.other.IsNone;
        }
    }
}