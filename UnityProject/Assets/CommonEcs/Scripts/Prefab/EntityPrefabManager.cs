using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public struct EntityPrefabManager : IComponentData {
        private LinearHashMap128<int, Entity> prefabMap;

        /// <summary>
        /// Adds a prefab
        /// </summary>
        /// <param name="id"></param>
        /// <param name="prefab"></param>
        public void Add(in FixedString64Bytes id, in Entity prefab) {
            this.prefabMap.AddOrSet(id.GetHashCode(), prefab);
        }

        /// <summary>
        /// Resolves the prefab by its text ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ValueTypeOption<Entity> GetPrefab(in FixedString64Bytes id) {
            return this.prefabMap.Find(id.GetHashCode());
        }

        /// <summary>
        /// Resolves the prefab by ID hashcode. This is used when only the id hashcode is stored in components.
        /// </summary>
        /// <param name="idHashCode"></param>
        /// <returns></returns>
        public ValueTypeOption<Entity> GetPrefab(int idHashCode) {
            return this.prefabMap.Find(idHashCode);
        }

        public int Count => this.prefabMap.Count;

        public LinearHashMapBucket128<int, Entity>.Enumerator Entries => this.prefabMap.Entries;
    }
}