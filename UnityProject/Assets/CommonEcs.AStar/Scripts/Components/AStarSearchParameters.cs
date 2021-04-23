using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public readonly struct AStarSearchParameters : IComponentData {
        public readonly int3 start;
        public readonly int3 goal;

        public AStarSearchParameters(int3 start, int3 goal) {
            this.start = start;
            this.goal = goal;
        }
    }
}