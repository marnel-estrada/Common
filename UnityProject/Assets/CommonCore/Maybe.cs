using System;

namespace Common {
    /// <summary>
    /// The "maybe" monad, enforced by Unity assertions
    /// </summary>
    /// 
    /// <example>
    /// Create one with a value like this:
    /// <code>
    ///   Maybe{int} maybe = new Maybe{int}(123);
    /// </code>
    /// 
    /// Create one without a value like this:
    /// <code>
    ///   Maybe{int} maybe = Maybe{int}.Nothing;
    /// </code>
    /// </example>
    /// 
    /// <author>
    /// Jackson Dunstan, https://JacksonDunstan.com/articles/4930
    /// </author>
    /// 
    /// <license>
    /// MIT
    /// </license>
    public struct Maybe<T> {
        /// <summary>
        /// If the value is set. True when the constructor is called. False
        /// otherwise, such as when `default(T)` is called.
        /// </summary>
        private readonly bool hasValue;

        /// <summary>
        /// The value passed to the constructor or `default(T)` otherwise
        /// </summary>
        private readonly T value;

        /// <summary>
        /// Create the <see cref="Maybe{T}"/> with a set value
        /// </summary>
        /// 
        /// <param name="value">
        /// Value to set
        /// </param>
        public Maybe(T value) {
            this.value = value;
            this.hasValue = true;
        }

        /// <summary>
        /// Create a <see cref="Maybe{T}"/> with no set value
        /// </summary>
        /// 
        /// <value>
        /// A <see cref="Maybe{T}"/> with no set value
        /// </value>
        public static Maybe<T> Nothing {
            get {
                return default(Maybe<T>);
            }
        }

        /// <summary>
        /// Convert the <see cref="Maybe{T}"/> to its value. Throws an exception to
        /// ensure that the value is set.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the converted <see cref="Maybe{T}"/>
        /// </returns>
        /// 
        /// <param name="maybe">
        /// The <see cref="Maybe{T}"/> to get the value of
        /// </param>
        public static explicit operator T(Maybe<T> maybe) {
            Assertion.Assert(maybe.hasValue, "Can't convert Maybe<T> to T when not set");
            return maybe.Value;
        }

        /// <summary>
        /// Whether a value is set
        /// </summary>
        /// 
        /// <value>
        /// If a value is set, i.e. by the constructor
        /// </value>
        public bool HasValue {
            get {
                return this.hasValue;
            }
        }

        /// <summary>
        /// Get the value passed to the construtor or assert if the constructor was
        /// not called, e.g. by creating with `default(T)`.
        /// </summary>
        /// 
        /// <value>
        /// The set value
        /// </value>
        public T Value {
            get {
                Assertion.Assert(this.hasValue, "Can't get Maybe<T> value when not set");
                if (!this.hasValue) {
                    // Force throw exception since Assert() does not throw exception in production
                    throw new Exception("Invalid Maybe usage. Trying to access a value that does not exits.");
                }
                return this.value;
            }
        }

        /// <summary>
        /// Get the value passed to the construtor or `default(T)` if the
        /// constructor was not called, e.g. by creating with `default(T)`.
        /// </summary>
        /// 
        /// <returns>
        /// The value if set or `default(T)` if not set
        /// </returns>
        public T GetValueOrDefault() {
            return this.hasValue ? this.value : default(T);
        }

        /// <summary>
        /// Get the value passed to the construtor or the parameter if the
        /// constructor was not called, e.g. by creating with `default(T)`.
        /// </summary>
        /// 
        /// <returns>
        /// The value if set or the parameter if not set
        /// </returns>
        /// 
        /// <param name="defaultValue">
        /// Value to return if the value isn't set
        /// </param>
        public T GetValueOrDefault(T defaultValue) {
            return this.hasValue ? this.value : defaultValue;
        }
    }
}