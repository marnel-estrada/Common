using System;
using System.Diagnostics.Contracts;
using Common;
using Unity.Burst;

namespace CommonEcs {
    /// <summary>
    /// This is the same for Option but for value types. We used a different type such that
    /// it can be used inside jobs.
    ///
    /// We don't provide an accessor to the value so that user would be forced to use
    /// Match(), ValueOr() or ValueOrError().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    #if UNITY_EDITOR
    public struct ValueTypeOption<T> : IEquatable<ValueTypeOption<T>> where T : struct {
        // Set these values to non-readonly public variables so that we can see and select them in 
        // Unity's Component Inspector
        public T value;
        public bool hasValue;
        #else
    public readonly struct ValueTypeOption<T> : IEquatable<ValueTypeOption<T>> where T : struct, IEquatable<T> {
        private readonly T value;
        private readonly bool hasValue;
        #endif

        // We use property here as static member variable doesn't work for Burst
        public static ValueTypeOption<T> None => new();

        public static ValueTypeOption<T> Some(in T value) {
            return new ValueTypeOption<T>(value);
        }

        private ValueTypeOption(in T value) {
            this.value = value;
            this.hasValue = true;
        }

        public bool IsSome => this.hasValue;

        public bool IsNone => !this.hasValue;

        /// <summary>
        /// This is used for matching using a struct without incurring garbage
        /// </summary>
        /// <param name="matcher"></param>
        /// <typeparam name="TMatcher"></typeparam>
        public void Match<TMatcher>(TMatcher matcher) where TMatcher : struct, IOptionMatcher<T> {
            if (this.IsSome) {
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
        /// <typeparam name="TReturnType"></typeparam>
        /// <returns></returns>
        [Pure]
        public TReturnType Match<TMatcher, TReturnType>(TMatcher matcher)
            where TMatcher : struct, IFuncOptionMatcher<T, TReturnType> {
            return this.IsSome ? matcher.OnSome(this.value) : matcher.OnNone();
        }

        [Pure]
        public T ValueOr(T other) {
            return this.IsSome ? this.value : other;
        }

        [Pure]
        public T ValueOrError() {
            if (this.IsSome) {
                return this.value;
            }

            throw new Exception("Trying to access value from a None option.");
        }

        public bool Equals(ValueTypeOption<T> other) {
            return this.value.Equals(other.value) && this.hasValue == other.hasValue;
        }

        public override bool Equals(object? obj) {
            return obj is ValueTypeOption<T> other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.value.GetHashCode() * 397) ^ this.hasValue.GetHashCode();
            }
        }

        public static bool operator ==(ValueTypeOption<T> left, ValueTypeOption<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(ValueTypeOption<T> left, ValueTypeOption<T> right) {
            return !left.Equals(right);
        }
    }
}