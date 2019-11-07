namespace Common {
    /// <summary>
    /// This is copied from Rust's Option feature. This could be another way to handle
    /// nullable types.
    ///
    /// We don't provide an accessor to the value so that user would be forced to use
    /// Match().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Option<T> where T : class {
        public static readonly Option<T> NONE = new Option<T>();

        /// <summary>
        /// Returns an option with a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Option<T> Some(T value) {
            return new Option<T>(value);
        }
        
        private readonly bool hasValue;
        private readonly T value;

        private Option(T value) {
            this.value = value;
            Assertion.AssertNotNull(this.value); // Can't be null
            
            this.hasValue = true;
        }

        public bool IsSome {
            get {
                return this.hasValue;
            }
        }

        public bool IsNone {
            get {
                return !this.hasValue;
            }
        }

        public void Match(IOptionMatcher<T> matcher) {
            if (this.hasValue) {
                matcher.OnSome(this.value);
            } else {
                matcher.OnNone();
            }
        }

        public bool ReferenceEquals(T other) {
            return this.value == other;
        }
    }
}