using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to wrap NativeHashMap so we don't pass it around when used in jobs
    /// </summary>
    public struct EntityPrefabResolver {
        private NativeHashMap<FixedString64Bytes, Entity> map;

        public EntityPrefabResolver(NativeHashMap<FixedString64Bytes, Entity> map) {
            this.map = map;
        }

        public Entity GetEntityPrefab(FixedString64Bytes id) {
            if (this.map.TryGetValue(id, out Entity prefabEntity)) {
                return prefabEntity;
            }
            
            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }
    }
}