using System;

using Common;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    public struct SetSpriteLayerMaterial : ISharedComponentData, IEquatable<SetSpriteLayerMaterial> {
        public readonly Entity layerEntity;
        public readonly Material newMaterial;
        
        private readonly int id;
        private static readonly IdGenerator GENERATOR = new IdGenerator(1);

        public SetSpriteLayerMaterial(Entity layerEntity, Material newMaterial) {
            this.layerEntity = layerEntity;
            Assertion.Assert(this.layerEntity != Entity.Null);
            
            this.newMaterial = newMaterial;
            
            this.id = GENERATOR.Generate();
        }

        public bool Equals(SetSpriteLayerMaterial other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is SetSpriteLayerMaterial other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(SetSpriteLayerMaterial left, SetSpriteLayerMaterial right) {
            return left.Equals(right);
        }

        public static bool operator !=(SetSpriteLayerMaterial left, SetSpriteLayerMaterial right) {
            return !left.Equals(right);
        }
    }
}