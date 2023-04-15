using System.Collections.Generic;

using Common;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    //[RequireComponent(typeof(ConvertToEntity))]
    public class EntityPrefabManagerPopulator : MonoBehaviour {
        [SerializeField]
        private EntityPrefabItemsHolder items;

        // public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        //     Assertion.NotNull(this.items);
        //     
        //     IReadOnlyList<EntityPrefabItem> prefabs = this.items.Prefabs;
        //     for (int i = 0; i < prefabs.Count; ++i) {
        //         referencedPrefabs.Add(prefabs[i].prefab);
        //     }
        // }
        //
        // public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        //     Assertion.NotNull(this.items);
        //
        //     EntityPrefabManagerSystem manager =
        //         World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityPrefabManagerSystem>();
        //     
        //     IReadOnlyList<EntityPrefabItem> prefabs = this.items.Prefabs;
        //     for (int i = 0; i < prefabs.Count; ++i) {
        //         EntityPrefabItem item = prefabs[i];
        //         Entity entityPrefab = conversionSystem.GetPrimaryEntity(item.prefab);
        //         manager.Add(item.id, entityPrefab);
        //     }
        // }
    }
}
