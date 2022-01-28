using Unity.Entities;

namespace CommonEcs {
    public readonly struct PathfindingParameters : IComponentData {
        public readonly GridCoord3 start;
        public readonly ValueTypeOption<GridCoord3> goal;

        /// <summary>
        /// Constructor with start only. This will be used by Dijsktra search where the goal is identfied
        /// by a goal identifier.
        /// </summary>
        /// <param name="start"></param>
        public PathfindingParameters(GridCoord3 start) {
            this.start = start;
            this.goal = ValueTypeOption<GridCoord3>.None;
        }

        public PathfindingParameters(GridCoord3 start, GridCoord3 goal) {
            this.start = start;
            this.goal = ValueTypeOption<GridCoord3>.Some(goal);
        }
    }
}