using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A common Baker that adds a single component by type.
    /// </summary>
    /// <typeparam name="TAuthoring"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    public class SingleComponentBaker<TAuthoring, TComponent> : Baker<TAuthoring>
        where TAuthoring : Component
        where TComponent : unmanaged, IComponentData {
        public override void Bake(TAuthoring authoring) {
            AddComponent<TComponent>(this.GetPrimaryEntity());
        }
    }
}