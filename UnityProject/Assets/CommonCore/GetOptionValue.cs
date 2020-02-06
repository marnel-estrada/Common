using System;

namespace Common {
    [Obsolete("Should no longer be used. Will remove this soon.")]
    public class GetOptionValue<T> : IFuncOptionMatcher<T, T> where T : class {
        public T OnSome(T value) {
            return value;
        }

        public T OnNone() {
            Assertion.Assert(false, "Can't get value. There's no value.");
            return default(T);
        }
    }
}