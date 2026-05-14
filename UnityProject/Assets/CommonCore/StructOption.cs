using System;
using System.Diagnostics.Contracts;

namespace Common {
    /// <summary>
    /// Option for structs but not unmanaged. Used for structs that contains managed types.
    /// </summary>
    public readonly struct StructOption<T> where T : struct {
        public static StructOption<T> None => new();
        public static StructOption<T> Some(T value) => new(value);
        
        private readonly T value;
        private readonly bool hasValue;

        private StructOption(T value) {
            this.value = value;
            this.hasValue = true;
        }
        
        public bool IsSome => this.hasValue;

        public bool IsNone => !this.hasValue;
        
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
    }
}