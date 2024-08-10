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
        private static readonly IdGenerator GENERATOR = new IdGenerator(1);

        public MeshRendererVessel(Entity spriteLayerEntity, string name, Material material, int layer, int sortingLayerId, int sortingOrder) {
            this.id = GENERATOR.Generate();
            this.internalInstance = new Internal();
            this.internalInstance.Init(spriteLayerEntity, name, this.id, material, layer, sortingLayerId, sortingOrder);
        }
        
        public Mesh Mesh {
            get => this.internalInstance.Mesh;

            set => this.internalInstance.Mesh = value;
        }

        public Entity SpriteLayerEntity => this.internalInstance.SpriteLayerEntity;

        public Material Material {
            get => this.internalInstance.Material;

            set => this.internalInstance.Material = value;
        }
        
        public bool Enabled {
            get => this.internalInstance.Enabled;

            set => this.internalInstance.Enabled = value;
        }
        
        private class Internal {
            private Entity spriteLayerEntity; // the layer that owns this vessel
            private GameObject gameObject;
            private MeshFilter meshFilter;
            private MeshRenderer meshRenderer;

            // Initializer
            public void Init(Entity spriteLayerEntity, string name, int id, Material material, int layer,
                int sortingLayerId, int sortingOrder) {
                this.spriteLayerEntity = spriteLayerEntity;
                
                if (this.gameObject != null) {
                    // A current one exists. Let's destroy it first.
                    Clear();
                }
                
                this.gameObject = new GameObject($"MeshRendererVessel.{name}.{id}");
                this.gameObject.layer = layer;
                this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
                
                this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
                this.meshRenderer.material = material;
                this.meshRenderer.sortingLayerID = sortingLayerId;
                this.meshRenderer.sortingOrder = sortingOrder;
            }

            private void Clear() {
                Object.Destroy(this.gameObject);
                this.gameObject = null;
                this.meshFilter = null;
                this.meshRenderer = null;
            }

            public Mesh Mesh {
                get => this.meshFilter.mesh;

                set => this.meshFilter.mesh = value;
            }

            public Entity SpriteLayerEntity => this.spriteLayerEntity;

            public Material Material {
                get => this.meshRenderer.material;

                set => this.meshRenderer.material = value;
            }

            public bool Enabled {
                get => this.gameObject.activeSelf;

                set => this.gameObject.SetActive(value);
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