using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonEcs {
    [CreateAssetMenu(menuName = "CommonEcs/EntityPrefabItemsHolder")]
    public class EntityPrefabItemsHolder : ScriptableObject {
        [SerializeField]
        [Searchable]
        private List<EntityPrefabItem> prefabs;

        public IReadOnlyList<EntityPrefabItem> Prefabs {
            get {
                return this.prefabs;
            }
        }
    }
}