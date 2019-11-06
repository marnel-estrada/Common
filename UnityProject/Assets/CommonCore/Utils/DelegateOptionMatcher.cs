namespace Common {
    public class DelegateOptionMatcher<T> : IOptionMatcher<T> where T : class {
        public delegate void SomeProcessor(T value);

        public delegate void NoneProcessor();

        private readonly SomeProcessor someProcessor;
        private readonly NoneProcessor noneProcessor;

        public DelegateOptionMatcher(SomeProcessor someProcessor, NoneProcessor noneProcessor) {
            this.someProcessor = someProcessor;
            this.noneProcessor = noneProcessor;
        }

        public DelegateOptionMatcher(SomeProcessor someProcessor) : this(someProcessor, null) {
        }

        public void OnSome(T value) {
            this.someProcessor(value);
        }

        public void OnNone() {
            this.noneProcessor?.Invoke();
        }
    }
}