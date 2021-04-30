using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Contains common consideration data.
    /// </summary>
    public struct Consideration : IComponentData {
        public int rank;
        public float bonus;
        public float multiplier;
    }
}