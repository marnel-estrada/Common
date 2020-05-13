namespace Common {
    public static class OptionUtils<T> where T : class {
        public static bool Equals(Option<T> a, Option<T> b) {
            return a.Match<OptionEqualityMatcher<T>, bool>(new OptionEqualityMatcher<T>(b));
        }
    }
}