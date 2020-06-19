using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to wrap NativeHashMap so we don't pass it around when used in jobs
    /// </summary>
    public struct EntityPrefabResolver {
        private NativeHashMap<int, Entity> map;

        public EntityPrefabResolver(NativeHashMap<int, Entity> map) {
            this.map = map;
        }

        public Entity GetEntityPrefab(FixedString64 id) {
            if (this.map.TryGetValue(id.GetHashCode(), out Entity prefabEntity)) {
                return prefabEntity;
            }
            
            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }
    }
}