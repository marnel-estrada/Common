﻿using System;

using Unity.Entities;

using Common;

using UnityEngine;

#nullable enable

namespace CommonEcs {
    public struct TimeReference : ISharedComponentData, IEquatable<TimeReference> {
        private Internal instance;

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

        public float DeltaTime {
            get {
                return Time.unscaledDeltaTime * this.instance.timeScale;
            }
        }

        /// <summary>
        /// This can called on domain reload
        /// </summary>
        public void Reset() {
            this.TimeScale = 1.0f;
        }
        
        public bool Equals(TimeReference other) {
            return Equals(this.instance, other.instance);
        }

        public override bool Equals(object obj) {
            return obj is TimeReference other && Equals(other);
        }

        public override int GetHashCode() {
            return this.instance.GetHashCode();
        }

        public static bool operator ==(TimeReference left, TimeReference right) {
            return left.Equals(right);
        }

        public static bool operator !=(TimeReference left, TimeReference right) {
            return !left.Equals(right);
        }

        // We use a class internally so we don't have to access it through a chunk if we want to modify it.
        private struct Internal : IEquatable<Internal> {
            public readonly byte id;
            public float timeScale;

            public Internal(byte id) {
                this.id = id;
                Assertion.IsTrue(this.id > 0); // Zero is reserved to identify a non existing TimeReference
            
                this.timeScale = 1.0f;
            }

            public bool Equals(Internal other) {
                return this.id == other.id;
            }

            public override bool Equals(object obj) {
                if (obj.GetType() != GetType()) {
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