using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A base MonoBehaviour that can be added to prefabs so that the converted entity has the
    /// specified buffer element type. We did it this way because GenerateAuthoringComponent does
    /// not work for IBufferElementData.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BufferElementDataConverter<T> : MonoBehaviour
        where T : unmanaged, IBufferElementData {
        // Provide the baker
        internal class InternalBaker<T> : Baker<BufferElementDataConverter<T>> where T : unmanaged, IBufferElementData {
            public override void Bake(BufferElementDataConverter<T> authoring) {
                AddBuffer<T>();
            }
        }
    }
}