namespace Common {
    public class DelegateFuncOptionMatcher<TWrappedType, TReturnType> : IFuncOptionMatcher<TWrappedType, TReturnType> 
        where TWrappedType : class {
        public delegate TReturnType SomeProcessor(TWrappedType value);

        public delegate TReturnType NoneProcessor();

        private readonly SomeProcessor someProcessor;
        private readonly NoneProcessor noneProcessor;

        public DelegateFuncOptionMatcher(SomeProcessor someProcessor, NoneProcessor noneProcessor) {
            this.someProcessor = someProcessor;
            this.noneProcessor = noneProcessor;
        }
        
        public TReturnType OnSome(TWrappedType value) {
            return this.someProcessor(value);
        }

        public TReturnType OnNone() {
            return this.noneProcessor();
        }
    }
}