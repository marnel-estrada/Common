using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A component that identifies a certain sprite to add to sprite layer
    /// </summary>
    public struct AddToSpriteLayer : IComponentData {
        public Entity layerEntity;

        public AddToSpriteLayer(Entity layerEntity) {
            this.layerEntity = layerEntity;
        }
    }

    public class AddToSpriteLayerAuthoring : MonoBehaviour {
        public GameObject layerEntity;
        
        internal class Baker : Baker<AddToSpriteLayerAuthoring> {
            public override void Bake(AddToSpriteLayerAuthoring authoring) {
                Entity primaryEntity = GetEntity(TransformUsageFlags.Renderable);
                Entity layerEntity = GetEntity(authoring.layerEntity, TransformUsageFlags.None);
                AddComponent(primaryEntity, new AddToSpriteLayer(layerEntity));
            }
        }
    }
}
