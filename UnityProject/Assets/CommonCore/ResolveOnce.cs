using System;

namespace Common {
    /// <summary>
    /// A utility struct which is used to resolve a value just once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ResolveOnce<T> {
        private readonly Func<T> resolver;

        private bool resolved;
        private T value;

        public ResolveOnce(Func<T> resolver) : this() {
            this.resolver = resolver;
            this.resolved = false;
        }

        public T Value {
            get {
                if (!this.resolved) {
                    // Not resolved yet
                    this.value = this.resolver(); // Invoke the functor
                    this.resolved = true;
                }

                return this.value;
            }
        }

        public bool Resolved {
            get {
                return this.resolved;
            }
        }
    }
}