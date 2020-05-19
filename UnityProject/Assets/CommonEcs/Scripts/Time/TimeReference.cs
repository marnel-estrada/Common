using System;

using Unity.Entities;

using Common;

namespace CommonEcs {
    public struct TimeReference : ISharedComponentData, IEquatable<TimeReference> {
        private readonly Internal instance;

        public TimeReference(byte id) {
            this.instance = new Internal(id);
        }

        public float TimeScale {
            get {
                return this.instance.timeScale;
            }

            set {
                this.instance.timeScale = value;
            }
        }
        
        public bool IsNull {
            get {
                return this.instance == null;
            }
        }
        
        public bool Equals(TimeReference other) {
            return Equals(this.instance, other.instance);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is TimeReference other && Equals(other);
        }

        public override int GetHashCode() {
            return (this.instance != null ? this.instance.GetHashCode() : 0);
        }

        public static bool operator ==(TimeReference left, TimeReference right) {
            return left.Equals(right);
        }

        public static bool operator !=(TimeReference left, TimeReference right) {
            return !left.Equals(right);
        }

        // We use a class internally so we don't have to access it through a chunk if we want to modify it.
        private class Internal : IEquatable<Internal> {
            public readonly byte id;
            public float timeScale;

            public Internal(byte id) {
                this.id = id;
                Assertion.IsTrue(this.id > 0); // Zero is reserved to identify a non existing TimeReference
            
                this.timeScale = 1.0f;
            }

            public bool Equals(Internal other) {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return this.id == other.id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != this.GetType()) {
                    return false;
                }

                return Equals((Internal) obj);
            }

            public override int GetHashCode() {
                return this.id.GetHashCode();
            }
        }
    }
}