using System;

using Unity.Collections;

namespace CommonEcs.DotsFsm {
    public readonly struct FsmEvent : IEquatable<FsmEvent> {
        public readonly int id;

        public FsmEvent(int id) {
            this.id = id;
        }

        public FsmEvent(in FixedString32Bytes stringId) : this(stringId.GetHashCode()) {
        }
        
        public FsmEvent(in FixedString64Bytes stringId) : this(stringId.GetHashCode()) {
        }

        public bool Equals(FsmEvent other) {
            return this.id == other.id;
        }

        public override bool Equals(object? obj) {
            return obj is FsmEvent other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(FsmEvent left, FsmEvent right) {
            return left.Equals(right);
        }

        public static bool operator !=(FsmEvent left, FsmEvent right) {
            return !left.Equals(right);
        }
    }
}