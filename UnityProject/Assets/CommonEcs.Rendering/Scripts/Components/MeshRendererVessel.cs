using System;

using Common;

using Unity.Entities;

using UnityEngine;

using Object = UnityEngine.Object;

namespace CommonEcs {
    /// <summary>
    /// A shared component that holds a GameObject that has a MeshRenderer
    /// </summary>
    public struct MeshRendererVessel : ISharedComponentData, IEquatable<MeshRendererVessel> {
        private readonly Internal internalInstance;
        
        private readonly int id;
        private static readonly IdGenerator GENERATOR = new IdGenerator();

        public MeshRendererVessel(Material material, int layer, int sortingLayerId) {
            this.internalInstance = new Internal();
            this.internalInstance.Init(material, layer, sortingLayerId);
            this.id = GENERATOR.Generate();
        }
        
        public Mesh Mesh {
            get {
                return this.internalInstance.Mesh;
            }

            set {
                this.internalInstance.Mesh = value;
            }
        }
        
        private class Internal {
            private GameObject gameObject;
            private MeshFilter meshFilter;
            private MeshRenderer meshRenderer;

            // Initializer
            public void Init(Material material, int layer, int sortingLayerId) {
                if (this.gameObject != null) {
                    // A current one exists. Let's destroy it first.
                    Clear();
                }
                
                this.gameObject = new GameObject("MeshRendererVessel." + material.name);
                this.gameObject.layer = layer;
                this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
                
                this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
                this.meshRenderer.material = material;
                this.meshRenderer.sortingLayerID = sortingLayerId;
            }

            private void Clear() {
                Object.Destroy(this.gameObject);
                this.gameObject = null;
                this.meshFilter = null;
                this.meshRenderer = null;
            }

            public Mesh Mesh {
                get {
                    return this.meshFilter.mesh;
                }

                set {
                    this.meshFilter.mesh = value;
                }
            }
        }

        public bool Equals(MeshRendererVessel other) {
            return other.id == this.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is MeshRendererVessel other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(MeshRendererVessel left, MeshRendererVessel right) {
            return left.Equals(right);
        }

        public static bool operator !=(MeshRendererVessel left, MeshRendererVessel right) {
            return !left.Equals(right);
        }
    }
}