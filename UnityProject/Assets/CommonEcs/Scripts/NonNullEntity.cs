using System;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A utility that wraps an Entity such that it can't be null when used.
    /// We created this because components with GenerateAuthoringComponent can no longer have
    /// readonly fields. This is an added protection such that those components that needed an
    /// entity upon construction still has checking that the entity has a valid value.
    /// </summary>
    public struct NonNullEntity : IEquatable<NonNullEntity> {
        private Entity value;
        
        public NonNullEntity(in Entity entity) {
            if (entity == Entity.Null) {
                throw new Exception("Passed entity can't be null");
            }
            this.value = entity;
        }

        public static implicit operator NonNullEntity(in Entity entity) {
            return new NonNullEntity(entity);
        }

        public static implicit operator Entity(in NonNullEntity nonNullEntity) {
            return nonNullEntity.value;
        }

        public bool Equals(NonNullEntity other) {
            return this.value.Equals(other.value);
        }

        public override bool Equals(object? obj) {
            return obj is NonNullEntity other && Equals(other);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(NonNullEntity left, NonNullEntity right) {
            return left.Equals(right);
        }

        public static bool operator !=(NonNullEntity left, NonNullEntity right) {
            return !left.Equals(right);
        }
    }
}