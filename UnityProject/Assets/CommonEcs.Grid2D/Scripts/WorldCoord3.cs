using System;

using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is to differentiate world coordinates from grid coordinates.
    /// </summary>
    public struct WorldCoord3 : IEquatable<WorldCoord3> {
        public int3 value;

        public WorldCoord3(int3 value) {
            this.value = value;
        }

        public WorldCoord3(int x, int y, int z) : this(new int3(x, y, z)) {
        }

        public bool Equals(WorldCoord3 other) {
            return this.value.Equals(other.value);
        }

        public override bool Equals(object? obj) {
            return obj is WorldCoord3 other && Equals(other);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(WorldCoord3 left, WorldCoord3 right) {
            return left.Equals(right);
        }

        public static bool operator !=(WorldCoord3 left, WorldCoord3 right) {
            return !left.Equals(right);
        }
    }
}