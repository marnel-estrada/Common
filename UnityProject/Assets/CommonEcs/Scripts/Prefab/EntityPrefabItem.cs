using System;

using UnityEngine;

namespace CommonEcs {
    [Serializable]
    public struct EntityPrefabItem {
        public string id;
        public GameObject prefab;
    }
}
