using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public struct EntityPrefabManager : IComponentData {
        private LinearHashMap16<int, Entity> prefabMap;

        /// <summary>
        /// Adds a prefab
        /// </summary>
        /// <param name="id"></param>
        /// <param name="prefab"></param>
        public void Add(in FixedString64Bytes id, in Entity prefab) {
            this.prefabMap.AddOrSet(id.GetHashCode(), prefab);
        }

        public ValueTypeOption<Entity> GetPrefab(in FixedString64Bytes id) {
            return this.prefabMap.Find(id.GetHashCode());
        }
    }
}