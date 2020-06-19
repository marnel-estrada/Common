using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {

    /// <summary>
    /// A component that represents a cell in a 2D grid
    /// </summary>
    public struct Cell2D : IComponentData {
        public readonly float2 center;
        public readonly float2 size;
        public readonly int2 worldCoordinate; // This is the coordinate where (0, 0) is at the center
        public readonly int2 gridCoordinate; // This is the coordinate where (0, 0) is at the bottom left of the grid
        public readonly Entity grid; // Reference to the grid that owns this cell
        public readonly int index; // Index to the list of cells in the grid

        public Cell2D(Entity grid, int2 gridCoordinate, int2 worldCoordinate, float2 size, int index, float2 center) {
            this.grid = grid;
            this.gridCoordinate = gridCoordinate;
            this.worldCoordinate = worldCoordinate;
            this.size = size;
            this.center = center;
            this.index = index;
        }

        public float2 BottomCenter {
            get {
                float halfHeight = this.size.y * 0.5f;
                return new float2(this.center.x, this.center.y - halfHeight);
            }
        }

        public float2 BottomLeft {
            get {
                float halfWidth = this.size.x * 0.5f;
                float halfHeight = this.size.y * 0.5f;
                return new float2(this.center.x - halfWidth, this.center.y - halfHeight);
            }
        }

        public override string ToString() {
            return $"({this.worldCoordinate.x.ToString()}, {this.worldCoordinate.y.ToString()})";
        }
    }
}
