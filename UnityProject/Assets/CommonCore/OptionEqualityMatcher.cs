namespace Common {
    public class OptionEqualityMatcher<T> : IFuncOptionMatcher<T, bool> where T : class {
        private Option<T> other;

        public void Init(Option<T> other) {
            this.other = other;
        }

        private readonly CompareMatcher<T> compareMatcher = new CompareMatcher<T>();

        public bool OnSome(T value) {
            // Match the value for other
            this.compareMatcher.Init(value);
            return this.other.Match(this.compareMatcher);
        }

        public bool OnNone() {
            // Return true if the other is none as well
            return this.other.IsNone;
        }

        private class CompareMatcher<T> : IFuncOptionMatcher<T, bool> where T : class {
            private T otherValueToCompare;

            public void Init(T otherValueToCompare) {
                this.otherValueToCompare = otherValueToCompare;
            }

            public bool OnSome(T value) {
                return value == this.otherValueToCompare;
            }

            public bool OnNone() {
                // Return true if the other specified value is null (none as well)
                return this.otherValueToCompare == null;
            }
        }
    }
}