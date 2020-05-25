using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Holds the mapping from integer ID to the Entity prefab
    /// </summary>
    public class EntityPrefabPoolSystem : SystemBase {
        private NativeHashMap<int, Entity> map;
        
        protected override void OnCreate() {
            this.map = new NativeHashMap<int, Entity>(10, Allocator.Persistent);
        }

        /// <summary>
        /// Adds a prefab to maintain
        /// </summary>
        /// <param name="item"></param>
        public void Add(string id, Entity entityPrefab) {
            int key = id.GetHashCode(); // We use hash code as the key
            if (this.map.ContainsKey(key)) {
                throw new Exception($"The prefab pool already contains an entry for {id}");
            }

            this.map[key] = entityPrefab;
        }

        /// <summary>
        /// This is used when the map is required inside a job
        /// </summary>
        public NativeHashMap<int, Entity> EntityPrefabMap {
            get {
                return this.map;
            }
        }

        public Entity GetEntityPrefab(string id) {
            if (this.map.TryGetValue(id.GetHashCode(), out Entity prefabEntity)) {
                return prefabEntity;
            }
            
            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }

        protected override void OnUpdate() {
        }
    }
}