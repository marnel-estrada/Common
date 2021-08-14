using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A base MonoBehaviour that can be added to prefabs so that the converted entity has the
    /// specified buffer element type. We did it this way because GenerateAuthoringComponent does
    /// not work for IBufferElementData.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BufferElementDataConverter<T> : MonoBehaviour, IConvertGameObjectToEntity 
        where T : struct, IBufferElementData {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddBuffer<T>(entity);
        }
    }
}