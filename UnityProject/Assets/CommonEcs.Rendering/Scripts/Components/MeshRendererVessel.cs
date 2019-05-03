using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A shared component that holds a GameObject that has a MeshRenderer
    /// </summary>
    public struct MeshRendererVessel : ISharedComponentData {
        private Internal internalInstance;

        public MeshRendererVessel(Material material, int layer, int sortingLayerId) {
            this.internalInstance = new Internal();
            this.internalInstance.Init(material, layer, sortingLayerId);
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
    }
}