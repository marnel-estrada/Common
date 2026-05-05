using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    public class SourcePrefabAuthoring : MonoBehaviour {
        public string prefabName; // Maps to entity prefab library
        
        private class SourcePrefabAuthoringBaker : Baker<SourcePrefabAuthoring> {
            public override void Bake(SourcePrefabAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);

                if (string.IsNullOrWhiteSpace(authoring.prefabName)) {
                    AddComponent<SourcePrefab>(entity);
                } else {
                    AddComponent(entity, new SourcePrefab(authoring.prefabName.AsIntId()));
                }
            }
        }
    }
}