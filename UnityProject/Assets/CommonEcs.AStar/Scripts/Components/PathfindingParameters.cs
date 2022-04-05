using Unity.Entities;

namespace CommonEcs {
    public readonly struct PathfindingParameters : IComponentData {
        public readonly GridCoord3 start;
        public readonly ValueTypeOption<GridCoord3> goal;
        public readonly bool isDebug;

        /// <summary>
        /// Constructor with start only. This will be used by Dijsktra search where the goal is identified
        /// by a goal identifier.
        /// </summary>
        /// <param name="start"></param>
        public PathfindingParameters(GridCoord3 start, bool isDebug = false) {
            this.start = start;
            this.goal = ValueTypeOption<GridCoord3>.None;
            this.isDebug = isDebug;
        }

        public PathfindingParameters(GridCoord3 start, GridCoord3 goal, bool isDebug = false) {
            this.start = start;
            this.goal = ValueTypeOption<GridCoord3>.Some(goal);
            this.isDebug = isDebug;
        }
    }
}