using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Common {
    /// <summary>
    /// This is copied from Rust's Option feature. This could be another way to handle
    /// nullable types.
    ///
    /// We don't provide an accessor to the value so that user would be forced to use
    /// Match().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Option<T> : IEquatable<Option<T>> where T : class {
        public static readonly Option<T> NONE = new();

        /// <summary>
        /// Returns an option with a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Option<T> Some(T value) {
            return new Option<T>(value);
        }
        
        /// <summary>
        /// We provided an explicit AsOption() so it's clear that the method accepts a null or non null
        /// value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Option<T> AsOption(T value) {
            return new Option<T>(value);
        }
        
        private readonly bool hasValue;
        private readonly T? value;

        private Option(T? value) {
            this.value = value;
            
            // This means that value is considered None if the specified value is null
            this.hasValue = this.value != null;
        }

        public bool IsSome => this.hasValue;

        public bool IsNone => !this.hasValue;

        /// <summary>
        /// A utility matcher that only requires an Action<T>
        /// It only gets invoked if the option has a value
        /// This is just a shorthand so we don't have to make verbose
        /// DelegationOptionMatcher instances.
        /// </summary>
        /// <param name="matcher"></param>
        public void Match(Action<T> matcher) {
            if (this.hasValue) {
                matcher.Invoke(this.value);
            }
        }

        public void Match(IOptionMatcher<T> matcher) {
            if (this.hasValue) {
                matcher.OnSome(this.value);
            } else {
                matcher.OnNone();
            }
        }

        /// <summary>
        /// This is used for matching using a struct without incurring garbage
        /// </summary>
        /// <param name="matcher"></param>
        /// <typeparam name="TMatcher"></typeparam>
        public void Match<TMatcher>(TMatcher matcher) where TMatcher : struct, IOptionMatcher<T> {
            if (this.hasValue) {
                matcher.OnSome(this.value);
            } else {
                matcher.OnNone();
            }
        }

        [Pure]
        public TReturnType Match<TReturnType>(IFuncOptionMatcher<T, TReturnType> matcher) {
            return this.hasValue ? matcher.OnSome(this.value) : matcher.OnNone();
        }

        /// <summary>
        /// This is used for matching using a struct without incurring garbage
        /// We used a different name to avoid mistake of using Match() to structs which
        /// causes garbage by boxing.
        /// </summary>
        /// <param name="matcher"></param>
        /// <typeparam name="TMatcher"></typeparam>
        /// <typeparam name="TReturnType"></typeparam>
        /// <returns></returns>
        public TReturnType MatchExplicit<TMatcher, TReturnType>(TMatcher matcher)
            where TMatcher : struct, IFuncOptionMatcher<T, TReturnType> {
            return this.hasValue ? matcher.OnSome(this.value) : matcher.OnNone();
        } 

        public bool Equals(T otherValue) {
            return EqualityComparer<T>.Default.Equals(this.value, otherValue);
        }

        public bool Equals(Option<T> other) {
            return this.hasValue == other.hasValue && EqualityComparer<T>.Default.Equals(this.value, other.value);
        }

        public override bool Equals(object obj) {
            return obj is Option<T> other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.hasValue.GetHashCode() * 397) ^ this.value?.GetHashCode() ?? 0;
            }
        }

        public static bool operator ==(Option<T> left, Option<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Option<T> left, Option<T> right) {
            return !left.Equals(right);
        }

        public T ValueOr(T valueWhenNone) {
            return this.IsSome ? this.value : valueWhenNone;
        }

        public T ValueOrError() {
            return this.IsSome ? this.value : throw new Exception("Option has no value");
        }
    }
}