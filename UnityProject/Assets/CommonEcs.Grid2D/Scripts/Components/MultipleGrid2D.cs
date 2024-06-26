using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is like Grid2D but used for multiple floors or layers
    /// Its coordinates are 3D where the 3rd one refers to the floor
    /// </summary>
    public readonly struct MultipleGrid2D : IComponentData {
        public readonly WorldCoord3 minWorldCoordinate;
        public readonly WorldCoord3 maxWorldCoordinate;

        public readonly GridCoord3 minGridCoordinate;
        public readonly GridCoord3 maxGridCoordinate;
        
        public readonly int columns;
        public readonly int rows;

        // This is the number of floors/layers
        public readonly int levels;

        // The game may have floors below the ground floor which is represented by negative integers
        // However, the cells are still arranged starting at zero z position.
        // We need this minZCoordinate so we can compute the z position in the perspective of the grid.
        public readonly int minZCoordinate;

        public readonly float cellWidth;
        public readonly float cellHeight;

        public MultipleGrid2D(int columns, int rows, float cellWidth, float cellHeight, int levels, int minZCoordinate) {
            this.columns = columns;
            this.rows = rows;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.levels = levels;
            this.minZCoordinate = minZCoordinate;

            this.minGridCoordinate = new GridCoord3(new int3(0, 0, minZCoordinate));
            this.maxGridCoordinate = new GridCoord3(new int3(this.columns - 1, this.rows - 1, minZCoordinate + levels - 1));

            int columnHalf = columns >> 1;
            int rowHalf = rows >> 1;
            this.minWorldCoordinate = new WorldCoord3(new int3(-columnHalf, -rowHalf, minZCoordinate));
            this.maxWorldCoordinate = new WorldCoord3(new int3(columnHalf - 1, rowHalf - 1, minZCoordinate + levels - 1));
        }

        public int ToIndex(GridCoord3 gridCoordinate) {
            return ToIndex(gridCoordinate.value.x, gridCoordinate.value.y, gridCoordinate.value.z);
        }
        
        public int ToIndex(int x, int y, int z) {
            int countPerFloor = this.columns * this.rows;
            int zeroBasedZ = z - this.minZCoordinate;
            DotsAssert.IsTrue(zeroBasedZ >= 0);
            return (zeroBasedZ * countPerFloor) + (y * this.columns + x);
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
            return (index / (this.columns * this.rows)) + this.minZCoordinate;
        }

        public bool IsInsideAsWorld(int x, int y, int z) {
            if (x < this.minWorldCoordinate.value.x || x > this.maxWorldCoordinate.value.x) {
                return false;
            }
            
            if (y < this.minWorldCoordinate.value.y || y > this.maxWorldCoordinate.value.y) {
                return false;
            }
            
            if (z < this.minWorldCoordinate.value.z || z > this.maxWorldCoordinate.value.z) {
                return false;
            }

            return true;
        }

        public bool IsInsideAsGrid(int x, int y, int z) {
            if (x < this.minGridCoordinate.value.x || x > this.maxGridCoordinate.value.x) {
                return false;
            }
            
            if (y < this.minGridCoordinate.value.y || y > this.maxGridCoordinate.value.y) {
                return false;
            }
            
            if (z < this.minGridCoordinate.value.z || z > this.maxGridCoordinate.value.z) {
                return false;
            }

            return true;
        }

        public int TotalCellCount => this.rows * this.columns * this.levels;

        public int CellCountPerLevel => this.rows * this.columns;

        public WorldCoord3 ToWorldCoordinate(in GridCoord3 gridCoordinate) {
            return new WorldCoord3(new int3(gridCoordinate.value.xy + this.minWorldCoordinate.value.xy, gridCoordinate.value.z));
        }

        public GridCoord3 ToGridCoordinate(in WorldCoord3 worldCoordinate) {
            return new GridCoord3(new int3(worldCoordinate.value.xy - this.minWorldCoordinate.value.xy, worldCoordinate.value.z));
        }
    }
}