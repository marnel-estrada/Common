using Unity.Entities;

namespace Components {
    /// <summary>
    /// A component that stores the source prefab of an entity. It can be used in serialization
    /// to spawn such prefab. The idHashCode here is the hash code of the text ID of the prefab.
    /// </summary>
    public readonly struct SourcePrefab : IComponentData {
        public readonly int idHashCode;

        public SourcePrefab(int idHashCode) {
            this.idHashCode = idHashCode;
        }
    }
}