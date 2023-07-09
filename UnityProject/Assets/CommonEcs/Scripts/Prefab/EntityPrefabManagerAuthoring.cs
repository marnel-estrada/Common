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
        
        public class Baker : Baker<EntityPrefabManagerAuthoring> {
            public override void Bake(EntityPrefabManagerAuthoring authoring) {
                // Prevent null point exception when the component is still being added
                if (authoring.Items == null) {
                    return;
                }

                DependsOn(authoring.Items);
                
                EntityPrefabManager prefabManager = new();
         
                IReadOnlyList<EntityPrefabItem> prefabs = authoring.Items.Prefabs;
                for (int i = 0; i < prefabs.Count; ++i) {
                    EntityPrefabItem item = prefabs[i];
                    Entity entityPrefab = GetEntity(item.prefab, TransformUsageFlags.Dynamic);
                    prefabManager.Add(item.id, entityPrefab);
                }
                
                AddComponent(GetEntity(TransformUsageFlags.None), prefabManager);
            }
        }
    }
}