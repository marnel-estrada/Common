using System;

using Common;

namespace CommonEcs {
    /// <summary>
    /// This is the same for Option but for value types. We used a different type such that
    /// it can be used inside jobs.
    ///
    /// We don't provide an accessor to the value so that user would be forced to use
    /// Match().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ValueTypeOption<T> : IEquatable<ValueTypeOption<T>> where T : struct {
        // We use property here as static member variable doesn't work for Burst
        public static ValueTypeOption<T> None {
            get {
                return new ValueTypeOption<T>();
            }
        }
        
        public static ValueTypeOption<T> Some(T value) {
            return new ValueTypeOption<T>(value);
        }
        
        private readonly T value;
        private readonly byte hasValue;

        private ValueTypeOption(T value) {
            this.value = value;
            this.hasValue = 1;
        }

        public bool IsSome {
            get {
                return this.hasValue > 0;
            }
        }

        public bool IsNone {
            get {
                return this.hasValue <= 0;
            }
        }

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
        public TReturnType Match<TMatcher, TReturnType>(TMatcher matcher)
            where TMatcher : struct, IFuncOptionMatcher<T, TReturnType> {
            return this.IsSome ? matcher.OnSome(this.value) : matcher.OnNone();
        }

        public T ValueOr(T other) {
            return this.IsSome ? this.value : other;
        }

        public bool Equals(ValueTypeOption<T> other) {
            return this.hasValue == other.hasValue && this.value.Equals(other.value);
        }

        public override bool Equals(object obj) {
            return obj is ValueTypeOption<T> other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.hasValue.GetHashCode() * 397) ^ this.value.GetHashCode();
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