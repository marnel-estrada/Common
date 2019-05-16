using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public struct AStarSearchParameters : IComponentData {
        public readonly int2 start;
        public readonly int2 goal;

        public AStarSearchParameters(int2 start, int2 goal) {
            this.start = start;
            this.goal = goal;
        }
    }
}