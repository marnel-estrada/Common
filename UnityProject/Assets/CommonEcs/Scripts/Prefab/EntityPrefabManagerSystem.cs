using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Holds the mapping from integer ID to the Entity prefab
    /// </summary>
    public partial class EntityPrefabManagerSystem : SystemBase {
        private NativeHashMap<FixedString64Bytes, Entity> map;
        
        protected override void OnCreate() {
            this.map = new NativeHashMap<FixedString64Bytes, Entity>(10, Allocator.Persistent);
        }

        protected override void OnDestroy() {
            if (this.map.IsCreated) {
                this.map.Dispose();
            }
        }

        /// <summary>
        /// Adds a prefab to maintain
        /// </summary>
        /// <param name="item"></param>
        public void Add(FixedString64Bytes id, Entity entityPrefab) {
            if (this.map.ContainsKey(id)) {
                throw new Exception($"The prefab pool already contains an entry for {id}");
            }

            this.map[id] = entityPrefab;
        }

        /// <summary>
        /// This is used when the map is required inside a job
        /// </summary>
        public EntityPrefabResolver EntityPrefabResolver {
            get {
                return new EntityPrefabResolver(this.map);
            }
        }

        public Entity GetEntityPrefab(FixedString64Bytes id) {
            if (this.map.TryGetValue(id, out Entity prefabEntity)) {
                return prefabEntity;
            }
            
            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }

        protected override void OnUpdate() {
        }
    }
}