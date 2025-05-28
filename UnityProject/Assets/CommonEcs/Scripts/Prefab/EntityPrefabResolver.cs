using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to wrap NativeHashMap so we don't pass it around when used in jobs
    /// </summary>
    public struct EntityPrefabResolver {
        private EntityPrefabManager prefabManager;

        public EntityPrefabResolver(in EntityPrefabManager prefabManager) {
            this.prefabManager = prefabManager;
        }

        public Entity GetEntityPrefab(FixedString64Bytes id) {
            ValueTypeOption<Entity> prefab = this.prefabManager.GetPrefab(id);
            if (prefab.IsNone) {
                throw new Exception($"The prefab pool does not contain an entry for {id}");
            }

            return prefab.ValueOrError();
        }

        public Entity GetEntityPrefab(int idHashCode) {
            ValueTypeOption<Entity> prefab = this.prefabManager.GetPrefab(idHashCode);
            if (prefab.IsNone) {
                throw new Exception($"The prefab pool does not contain an entry for idHashCode {idHashCode}");
            }

            return prefab.ValueOrError();
        }
    }
}