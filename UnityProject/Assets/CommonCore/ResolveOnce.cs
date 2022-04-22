using System;

namespace Common {
    /// <summary>
    /// A utility struct which is used to resolve a value just once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ResolveOnce<T> {
        private Option<T> valueOption;
        private readonly Func<T> resolver;

        public ResolveOnce(Func<T> resolver) : this() {
            this.resolver = resolver;
        }

        public T Value {
            get {
                if (this.valueOption.IsSome) {
                    return this.valueOption.ValueOrError();
                }

                // Not resolved yet
                this.valueOption = Option<T>.AsOption(this.resolver()); // Invoke the functor
                Assertion.IsSome(this.valueOption);
                return this.valueOption.ValueOrError();
            }
        }

        public bool Resolved {
            get {
                return this.valueOption.IsSome;
            }
        }
    }
}