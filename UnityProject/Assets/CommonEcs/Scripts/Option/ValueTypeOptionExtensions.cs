using System;

namespace CommonEcs {
    public static class ValueTypeOptionExtensions {
        public static bool EqualsEquatable<T>(this ValueTypeOption<T> self, ValueTypeOption<T> other) where T : unmanaged, IEquatable<T> {
            if (self.IsNone && other.IsNone) {
                return true;
            }

            return self.IsSome && other.IsSome && self.value.Equals(other.value);
        }
        
        public static bool EqualsValue<T>(this ValueTypeOption<T> self, T otherValue) where T : unmanaged, IEquatable<T> {
            if (self.IsNone) {
                return false;
            }

            return self.IsSome && self.value.Equals(otherValue);
        }
    }
}