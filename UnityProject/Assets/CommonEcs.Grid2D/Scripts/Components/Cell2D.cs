using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {

    /// <summary>
    /// A component that represents a cell in a 2D grid
    /// </summary>
    public struct Cell2D : IComponentData {
        public readonly Entity grid; // Reference to the grid that owns this cell
        public readonly int2 worldCoordinate; // This is the coordinate where (0, 0) is at the center
        public readonly int2 gridCoordinate; // This is the coordinate where (0, 0) is at the bottom left of the grid
        public readonly float3 center;
        public readonly int index; // Index to the list of cells in the grid

        public Cell2D(Entity grid, int2 gridCoordinate, int2 worldCoordinate, int index, float3 center) {
            this.grid = grid;
            this.gridCoordinate = gridCoordinate;
            this.worldCoordinate = worldCoordinate;
            this.center = center;
            this.index = index;
        }

        public override string ToString() {
            return $"({this.worldCoordinate.x.ToString()}, {this.worldCoordinate.y.ToString()})";
        }
    }
}
