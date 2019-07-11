using Common;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    public struct SetSpriteLayerMaterial : ISharedComponentData {
        public readonly Entity layerEntity;
        public readonly Material newMaterial;

        public SetSpriteLayerMaterial(Entity layerEntity, Material newMaterial) {
            this.layerEntity = layerEntity;
            Assertion.Assert(this.layerEntity != Entity.Null);
            
            this.newMaterial = newMaterial;
        }
    }
}