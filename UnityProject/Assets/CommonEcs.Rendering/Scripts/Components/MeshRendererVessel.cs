using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A shared component that holds a GameObject that has a MeshRenderer
    /// </summary>
    public struct MeshRendererVessel : ISharedComponentData {
        private readonly Internal internalInstance;

        public MeshRendererVessel(Entity spriteLayerEntity, Material material, int layer, int sortingLayerId) {
            this.internalInstance = new Internal();
            this.internalInstance.Init(spriteLayerEntity, material, layer, sortingLayerId);
        }
        
        public Mesh Mesh {
            get {
                return this.internalInstance.Mesh;
            }

            set {
                this.internalInstance.Mesh = value;
            }
        }

        public Entity SpriteLayerEntity {
            get {
                return this.internalInstance.SpriteLayerEntity;
            }
        }

        public Material Material {
            get {
                return this.internalInstance.Material;
            }

            set {
                this.internalInstance.Material = value;
            }
        }
        
        public bool Enabled {
            get {
                return this.internalInstance.Enabled;
            }

            set {
                this.internalInstance.Enabled = value;
            }
        }
        
        private class Internal {
            private Entity spriteLayerEntity; // the layer that owns this vessel
            private GameObject gameObject;
            private MeshFilter meshFilter;
            private MeshRenderer meshRenderer;

            // Initializer
            public void Init(Entity spriteLayerEntity, Material material, int layer, int sortingLayerId) {
                this.spriteLayerEntity = spriteLayerEntity;
                
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

            public Entity SpriteLayerEntity {
                get {
                    return this.spriteLayerEntity;
                }
            }

            public Material Material {
                get {
                    return this.meshRenderer.material;
                }

                set {
                    this.meshRenderer.material = value;
                }
            }

            public bool Enabled {
                get {
                    return this.gameObject.activeSelf;
                }

                set {
                    this.gameObject.SetActive(value);
                }
            }
        }
    }
}