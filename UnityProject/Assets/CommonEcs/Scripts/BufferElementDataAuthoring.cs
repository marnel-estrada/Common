using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A base MonoBehaviour that can be added to prefabs so that the converted entity has the
    /// specified buffer element type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BufferElementDataAuthoring<T> : MonoBehaviour
        where T : unmanaged, IBufferElementData {
        /// <summary>
        /// Generic Bakers are currently not supported. Derive from this as baker of authoring classes that uses
        /// BufferElementDataAuthoring.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class Baker : Baker<BufferElementDataAuthoring<T>> {
            public override void Bake(BufferElementDataAuthoring<T> authoring) {
                AddBuffer<T>(this.GetPrimaryEntity());
            }
        }
    }
}