namespace Common {
    public static class OptionUtils<T> where T : class {
        private static readonly OptionEqualityMatcher<T> EQUALITY_MATCHER = new OptionEqualityMatcher<T>();
        
        public static bool Equals(Option<T> a, Option<T> b) {
            EQUALITY_MATCHER.Init(a);
            return b.Match(EQUALITY_MATCHER);
        }
    }
}