using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    public class EntityPrefabManagerAuthoring : MonoBehaviour {
        [SerializeField]
        private List<EntityPrefabItem> prefabs;

        public class Baker : Baker<EntityPrefabManagerAuthoring> {
            public override void Bake(EntityPrefabManagerAuthoring authoring) {
                EntityPrefabManager prefabManager = new();
         
                IReadOnlyList<EntityPrefabItem> prefabs = authoring.prefabs;
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