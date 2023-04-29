using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    public struct DebugEntity : IComponentData {
        public bool enabled;

        public DebugEntity(bool enabled) {
            this.enabled = enabled;
        }
    }

    public class DebugEntityAuthoring : MonoBehaviour {
        public bool enabled;
        
        internal class Baker : Baker<DebugEntityAuthoring> {
            public override void Bake(DebugEntityAuthoring authoring) {
                Entity primaryEntity = GetEntity(TransformUsageFlags.None);
                AddComponent(primaryEntity, new DebugEntity(authoring.enabled));
            }
        }
    }
}
