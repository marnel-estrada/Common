using System;

using Unity.Entities;

using UnityEngine;

using Common;

namespace CommonEcs {
    /// <summary>
    /// A sprite layer is just a collection of sprite managers
    /// It handles more than one sprite manager in case of overflow
    /// </summary>
    public struct SpriteLayer : ISharedComponentData, IEquatable<SpriteLayer> {
        public Entity owner;
        
        // Common sprite manager values
        public readonly Material material;
        public readonly int allocationCount;
        public int layer; // This is the layer in Unity
        public bool alwaysUpdateMesh;
        public bool useMeshRenderer; // Generates a MeshRenderer instead of using SpriteManagerRendererSystem
        
        private int sortingLayerId; // This is the ID
        private int sortingLayer; // Note that this is the value, not the ID

        public readonly SimpleList<Entity> spriteManagerEntities;
        
        private readonly int id;
        private static readonly IdGenerator ID_GENERATOR = new IdGenerator(1);

        // Can be optionally be set
        private string name;

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
            this.id = ID_GENERATOR.Generate();
            this.name = "(no name)";
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

        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }

        public void SetSortingLayerId(int sortingLayerId) {
            this.sortingLayerId = sortingLayerId;
            this.sortingLayer = UnityEngine.SortingLayer.GetLayerValueFromID(sortingLayerId);
        }

        public bool Equals(SpriteLayer other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is SpriteLayer other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(SpriteLayer left, SpriteLayer right) {
            return left.Equals(right);
        }

        public static bool operator !=(SpriteLayer left, SpriteLayer right) {
            return !left.Equals(right);
        }
    }
}
