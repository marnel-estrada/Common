using System;

using Unity.Mathematics;

namespace CommonEcs {
    public struct GridCoord3 : IEquatable<GridCoord3> {
        public int3 value;

        public GridCoord3(int3 value) {
            this.value = value;
        }
        
        public GridCoord3(int x, int y, int z) : this(new int3(x, y, z)) {
        }

        public bool Equals(GridCoord3 other) {
            return this.value.Equals(other.value);
        }

        public override bool Equals(object? obj) {
            return obj is GridCoord3 other && Equals(other);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(GridCoord3 left, GridCoord3 right) {
            return left.Equals(right);
        }

        public static bool operator !=(GridCoord3 left, GridCoord3 right) {
            return !left.Equals(right);
        }
    }
}