using System;
using Common;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Holds the mapping from integer ID to the Entity prefab
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class EntityPrefabManagerSystem : SystemBase {
        private Option<EntityPrefabManager> prefabManager;
        
        protected override void OnCreate() {
        }

        protected override void OnDestroy() {
        }

        // It is only prepared when the prefabManager has been resolved
        public bool IsPrepared {
            get {
                return this.prefabManager.IsSome;
            }
        }

        /// <summary>
        /// This is used when the map is required inside a job
        /// </summary>
        public EntityPrefabResolver EntityPrefabResolver {
            get {
                if (this.prefabManager.IsNone) {
                    throw new Exception("The prefab manager is not resolved yet.");
                }
                
                return new EntityPrefabResolver(this.prefabManager.ValueOrError());
            }
        }

        public Entity GetEntityPrefab(FixedString64Bytes id) {
            if (this.prefabManager.IsNone) {
                throw new Exception("The prefab manager is not resolved yet.");
            }

            ValueTypeOption<Entity> prefab = this.prefabManager.ValueOrError().GetPrefab(id);
            if (prefab.IsNone) {
                throw new Exception($"The prefab pool does not contain an entry for {id}");    
            }

            return prefab.ValueOrError();
        }

        public void RegisterPrefabManager(ref EntityPrefabManager prefabManager) {
            Debug.Log("The EntityPrefabManager is found.");
            this.prefabManager = Option<EntityPrefabManager>.AsOption(prefabManager);
        }

        protected override void OnUpdate() {
        }
    }
}