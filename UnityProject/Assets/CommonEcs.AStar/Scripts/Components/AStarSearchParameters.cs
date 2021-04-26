using Unity.Entities;

namespace CommonEcs {
    public readonly struct AStarSearchParameters : IComponentData {
        public readonly GridCoord3 start;
        public readonly GridCoord3 goal;

        public AStarSearchParameters(GridCoord3 start, GridCoord3 goal) {
            this.start = start;
            this.goal = goal;
        }
    }
}