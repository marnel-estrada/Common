using System.Diagnostics.Contracts;

namespace CommonEcs {
    using Unity.Entities;
    using Unity.Mathematics;

    using UnityEngine;

    public struct Grid2D : IComponentData {
        // The id of the grid
        // This may be used to identify a grid if there are multiple grids
        public int id;

        public readonly int columnCount;
        public readonly int rowCount;

        public readonly float cellWidth;
        public readonly float cellHeight;

        // We cache these values for faster calculation of grid cell position
        public readonly float gridWidth; // The whole grid width
        public readonly float gridHeight; // The whole grid height

        public readonly float2 cellStart; // The center of starting cell position at the bottom left

        // AABB of the grid
        public readonly float2 min;
        public readonly float2 max;

        public readonly int2 minCoordinate;
        public readonly int2 maxCoordinte;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        /// <param name="cellWidth"></param>
        /// <param name="cellHeight"></param>
        public Grid2D(int columnCount, int rowCount, float cellWidth, float cellHeight) {
            this.id = int.MinValue;
            this.columnCount = columnCount;
            this.rowCount = rowCount;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            
            this.gridWidth = cellWidth * columnCount;
            this.min.x = -this.gridWidth * 0.5f;
            this.max.x = this.gridWidth * 0.5f;
            this.cellStart.x = this.min.x + (cellWidth * 0.5f);

            this.gridHeight = cellHeight * rowCount;
            this.min.y = -this.gridHeight * 0.5f;
            this.max.y = this.gridHeight * 0.5f;
            this.cellStart.y = this.min.y + (cellHeight * 0.5f);

            int halfColumns = this.columnCount >> 1;
            int halfRows = this.rowCount >> 1;
            this.minCoordinate = new int2(-halfColumns, -halfRows);
            this.maxCoordinte = new int2(halfColumns - 1, halfRows - 1);
        }

        /// <summary>
        /// Computes the linear index of the specified coordinate as with respect to the dimensions of the grid
        /// </summary>
        /// <param name="gridCoordinate"></param>
        /// <returns></returns>
        public int AsIndex(int2 gridCoordinate) {
            return AsIndex(gridCoordinate.x, gridCoordinate.y);
        }

        /// <summary>
        /// Resolves the linear index using the specified coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int AsIndex(int x, int y) {
            return y * this.columnCount + x;
        }

        /// <summary>
        /// Computes the x grid coordinate from the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetX(int index) {
            return index % this.columnCount;
        }

        public int GetY(int index) {
            return index / this.columnCount;
        }

        /// <summary>
        /// Returns whether or not the specified coordinate is inside the grid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [Pure]
        public bool IsInsideGrid(int x, int y) {
            if (x < this.minCoordinate.x || x > this.maxCoordinte.x) {
                return false;
            }
            
            if (y < this.minCoordinate.y || y > this.maxCoordinte.y) {
                return false;
            }

            return true;
        }

        public int CellCount {
            get {
                return this.rowCount * this.columnCount;
            }
        }

        public int2 ToWorldCoordinate(int2 gridCoordinate) {
            return gridCoordinate + this.minCoordinate;
        }

        public int2 ToGridCoordinate(int2 worldCoordinate) {
            return worldCoordinate - this.minCoordinate;
        }
    }
}
