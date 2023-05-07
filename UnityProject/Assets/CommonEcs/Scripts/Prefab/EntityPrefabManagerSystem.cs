using System;
using Common;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Holds the mapping from integer ID to the Entity prefab
    /// </summary>
    public partial class EntityPrefabManagerSystem : SystemBase {
        private Option<EntityPrefabManager> prefabManager;
        
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<EntityPrefabManager>().Build(this);
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

        protected override void OnUpdate() {
            if (this.prefabManager.IsSome) {
                // Value was already resolved
                return;
            }

            if (this.query.CalculateEntityCount() > 0) {
                Debug.Log("The EntityPrefabManager is found.");
            }
            
            // Resolve value here using SystemAPI.GetSingleton()
            if (SystemAPI.TryGetSingleton(out EntityPrefabManager prefabManager)) {
                this.prefabManager = Option<EntityPrefabManager>.AsOption(prefabManager);
            }
        }
    }
}