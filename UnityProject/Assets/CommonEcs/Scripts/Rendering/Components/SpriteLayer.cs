using Unity.Entities;

using UnityEngine;

using Common;

namespace CommonEcs {
    /// <summary>
    /// A sprite layer is just a collection of sprite managers
    /// It handles more than one sprite manager in case of overflow
    /// </summary>
    public struct SpriteLayer : ISharedComponentData {
        public Entity owner;
        
        // Common sprite manager values
        public Material material;
        public int allocationCount;
        public int layer; // This is the layer in Unity
        public bool alwaysUpdateMesh;
        public bool useMeshRenderer; // Generates a MeshRenderer instead of using SpriteManagerRendererSystem
        
        private int sortingLayerId; // This is the ID
        private int sortingLayer; // Note that this is the value, not the ID

        public readonly SimpleList<Entity> spriteManagerEntities;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="material"></param>
        /// <param name="allocationCount"></param>
        public SpriteLayer(Entity owner, Material material, int allocationCount) {
            this.owner = owner;
            this.material = material;
            this.allocationCount = allocationCount;
            this.spriteManagerEntities = new SimpleList<Entity>(1);
            this.layer = 0;
            this.sortingLayer = 0;
            this.sortingLayerId = 0;
            this.alwaysUpdateMesh = false;
            this.useMeshRenderer = false;
        }

        public int SortingLayerId {
            get {
                return this.sortingLayerId;
            }
        }

        public int SortingLayer {
            get {
                return this.sortingLayer;
            }
        }

        public void SetSortingLayerId(int sortingLayerId) {
            this.sortingLayerId = sortingLayerId;
            this.sortingLayer = UnityEngine.SortingLayer.GetLayerValueFromID(sortingLayerId);
        }
    }
}
