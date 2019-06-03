using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// Mainly used for temporarily copying values from TransformAccess
    /// </summary>
    public struct TransformStash {
        public float3 position;
        public quaternion rotation;
    }
}