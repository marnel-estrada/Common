using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is like Grid2D but used for multiple floors or layers
    /// Its coordinates are 3D where the 3rd one refers to the floor
    /// </summary>
    public readonly struct MultipleGrid2D : IComponentData {
        // These are world coordinates
        public readonly int3 minCoordinate;
        public readonly int3 maxCoordinte;
        
        public readonly int columns;
        public readonly int rows;

        // This is the number of floors/layers
        public readonly int levels;

        public readonly float cellWidth;
        public readonly float cellHeight;

        public MultipleGrid2D(int columns, int rows, float cellWidth, float cellHeight, int levels) {
            this.columns = columns;
            this.rows = rows;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.levels = levels;

            int columnHalf = columns >> 1;
            int rowHalf = rows >> 1;
            this.minCoordinate = new int3(-columnHalf, -rowHalf, 0);
            this.maxCoordinte = new int3(columnHalf - 1, rowHalf - 1, levels - 1);
        }

        public int ToIndex(int3 gridCoordinate) {
            return ToIndex(gridCoordinate.x, gridCoordinate.y, gridCoordinate.z);
        }
        
        public int ToIndex(int x, int y, int z) {
            int countPerLayer = this.columns * this.rows;
            return (z * countPerLayer) + (y * this.columns + x);
        }
        
        public int GetX(int index) {
            // Normalize index into a layer
            int layerIndex = index % (this.columns * this.rows);
            return layerIndex % this.columns;
        }

        public int GetY(int index) {
            // Normalize index into a layer
            int layerIndex = index % (this.columns * this.rows);
            return layerIndex / this.rows;
        }

        public int GetZ(int index) {
            return index / (this.columns * this.rows);
        }

        public int TotalCellCount {
            get {
                return this.rows * this.columns * this.levels;
            }
        }

        public int CellCountPerLevel {
            get {
                return this.rows * this.columns;
            }
        }
        
        public int3 ToWorldCoordinate(int3 gridCoordinate) {
            return new int3(gridCoordinate.xy + this.minCoordinate.xy, gridCoordinate.z);
        }

        public int3 ToGridCoordinate(int3 worldCoordinate) {
            return new int3(worldCoordinate.xy - this.minCoordinate.xy, worldCoordinate.z);
        }
    }
}