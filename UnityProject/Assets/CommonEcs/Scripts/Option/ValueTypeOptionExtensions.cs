using System;

namespace CommonEcs {
    public static class ValueTypeOptionExtensions {
        public static bool EqualsEquatable<TEquatable>(this ValueTypeOption<TEquatable> self, ValueTypeOption<TEquatable> other) where TEquatable : struct, IEquatable<TEquatable> {
            if (self.IsNone && other.IsNone) {
                return true;
            }

            return self.IsSome && other.IsSome && self.value.Equals(other.value);
        }
        
        public static bool EqualsValue<TEquatable>(this ValueTypeOption<TEquatable> self, TEquatable otherValue) where TEquatable : struct, IEquatable<TEquatable> {
            if (self.IsNone) {
                return false;
            }

            return self.IsSome && self.value.Equals(otherValue);
        }
    }
}