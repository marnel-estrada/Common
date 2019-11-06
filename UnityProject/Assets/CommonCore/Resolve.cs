namespace Common {
    using System.Collections.Generic;

    /// <summary>
    /// A wrapper that can mark or unmark a value as resolved or not
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Resolve<T> {
        private T value;
        private bool resolved;

        public bool Resolved {
            get {
                return this.resolved;
            }
        }

        public T Value {
            get {
                // Value can only be retrieved if it was resolved
                Assertion.Assert(this.resolved);
                return this.value;
            }
            
            set {
                // Can only set if it wasn't resolved yet
                Assertion.Assert(!this.resolved);
                this.value = value;
            }
        }

        /// <summary>
        /// This is different from Value such that it doesn't check if it was resolved or not
        /// We may need this for cases where we reuse the existing value. We can't get it if we
        /// use Value.
        /// </summary>
        public T InternalValue {
            get {
                return this.value;
            }
        }

        public bool IsNullOrDefault {
            get {
                return EqualityComparer<T>.Default.Equals(this.value, default(T));
            }
        }

        /// <summary>
        /// Must call this method to mark that the value has been resolved
        /// Note that the value may be specified but can still be considered as unresolved
        /// For example, say we have a Rectangle class
        /// This class may be instantiated by the value is no longer correct if it was mark as unresolved
        /// This tells the client to resolve the new value which need not instantiate it
        /// </summary>
        public void MarkResolved() {
            this.resolved = true;
        }

        /// <summary>
        /// This can be invoked to mark the value as unresolved so that client code needs to resolve it again
        /// This can be used when resetting things
        /// </summary>
        public void UnmarkResolved() {
            this.resolved = false;
        }
    }
}
