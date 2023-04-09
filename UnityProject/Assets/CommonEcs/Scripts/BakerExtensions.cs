using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    public static class BakerExtensions {
        public static Entity GetPrimaryEntity<T>(this Baker<T> baker) where T : Component {
            return baker.GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}