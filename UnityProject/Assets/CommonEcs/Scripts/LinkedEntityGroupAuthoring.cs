using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// An authoring component that adds LinkedEntityGroup
    /// We do this so we could add it in prefab. It's not available in Unity.Entities.
    /// </summary>
    public class LinkedEntityGroupAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddBuffer<LinkedEntityGroup>(entity);
        }
    }
}