using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// An authoring component that adds LinkedEntityGroup
    /// We do this so we could add it in prefab. It's not available in Unity.Entities.
    /// </summary>
    public class LinkedEntityGroupAuthoring : MonoBehaviour {
        internal class Baker : Baker<LinkedEntityGroupAuthoring> {
            public override void Bake(LinkedEntityGroupAuthoring authoring) {
                AddBuffer<LinkedEntityGroup>();
            }
        }
    }
}