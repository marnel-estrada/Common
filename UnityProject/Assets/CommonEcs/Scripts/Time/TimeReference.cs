using System;

using Unity.Entities;

using Common;

using UnityEngine;

#nullable enable

namespace CommonEcs {
    public readonly struct TimeReference : ISharedComponentData, IEquatable<TimeReference> {
        public readonly int id;

        public TimeReference(int id) {
            this.id = id;
        }

        public bool Equals(TimeReference other) {
            return this.id == other.id;
        }

        public override bool Equals(object? obj) {
            return obj is TimeReference other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }
    }
}