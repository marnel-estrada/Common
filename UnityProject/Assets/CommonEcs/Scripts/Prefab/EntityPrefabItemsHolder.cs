using System.Collections.Generic;
using UnityEngine;

namespace CommonEcs {
    [CreateAssetMenu(menuName = "CommonEcs/EntityPrefabItemsHolder")]
    public class EntityPrefabItemsHolder : ScriptableObject {
        [SerializeField]
        private List<EntityPrefabItem> prefabs;

        public IReadOnlyList<EntityPrefabItem> Prefabs {
            get {
                return this.prefabs;
            }
        }
    }
}