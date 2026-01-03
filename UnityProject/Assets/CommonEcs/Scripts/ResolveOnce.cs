using System;
using CommonEcs;

namespace Common {
    /// <summary>
    /// A utility struct which is used to resolve a value just once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ResolveOnce<T> where T : unmanaged {
        private ValueTypeOption<T> valueOption;
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
                this.valueOption = ValueTypeOption<T>.Some(this.resolver()); // Invoke the functor
                DotsAssert.IsSome(this.valueOption);
                return this.valueOption.ValueOrError();
            }
        }

        public bool Resolved => this.valueOption.IsSome;
    }
}