using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    public class EntityPrefabManagerAuthoring : MonoBehaviour {
        [SerializeField]
        private EntityPrefabItemsHolder items;
        
        public EntityPrefabItemsHolder Items {
            get {
                return this.items;
            }
        }
        
        class Baker : Baker<EntityPrefabManagerAuthoring> {
            public override void Bake(EntityPrefabManagerAuthoring authoring) {
                EntityPrefabManager prefabManager = new EntityPrefabManager();
         
                IReadOnlyList<EntityPrefabItem> prefabs = authoring.Items.PrefabItems;
                for (int i = 0; i < prefabs.Count; ++i) {
                    EntityPrefabItem item = prefabs[i];
                    Entity entityPrefab = GetEntity(item.prefab);
                    prefabManager.Add(item.id, entityPrefab);
                }
                
                AddComponent(prefabManager);
            }
        }
    }
}