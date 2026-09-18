using System;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Unmanaged twin of <see cref="SpriteManager"/> used purely as a boxing-free query filter.
    /// Filtering a query by the managed SpriteManager value goes through Entities' managed
    /// hash/equals path which boxes the struct every call. Filtering by this unmanaged id instead
    /// uses the memcmp path and allocates nothing.
    /// </summary>
    public readonly struct SpriteManagerId : ISharedComponentData, IEquatable<SpriteManagerId> {
        public readonly int id;

        public SpriteManagerId(int id) {
            this.id = id;
        }

        public bool Equals(SpriteManagerId other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is SpriteManagerId other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(SpriteManagerId left, SpriteManagerId right) {
            return left.Equals(right);
        }

        public static bool operator !=(SpriteManagerId left, SpriteManagerId right) {
            return !left.Equals(right);
        }
    }
}
