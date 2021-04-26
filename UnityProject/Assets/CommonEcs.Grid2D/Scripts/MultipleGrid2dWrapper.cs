using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public readonly struct MultipleGrid2dWrapper {
        public readonly MultipleGrid2D grid;
        
        // Note that this array already contains all tiles for all levels
        public readonly NativeArray<EntityBufferElement> cellEntities;

        // This will be needed to convert from world to grid coordinates
        public readonly Aabb2 worldBoundingBox;

        public MultipleGrid2dWrapper(in MultipleGrid2D grid, in NativeArray<EntityBufferElement> cellEntities, 
            in Aabb2 worldBoundingBox) {
            this.grid = grid;
            this.cellEntities = cellEntities;
            this.worldBoundingBox = worldBoundingBox;
        }

        public ValueTypeOption<Entity> GetCellEntity(int x, int y, int z) {
            if (x < 0 || y < 0 || z < 0) {
                // Outside of map
                return ValueTypeOption<Entity>.None;
            }

            if (x >= this.grid.columns || y >= this.grid.rows) {
                // Outside of map
                return ValueTypeOption<Entity>.None;
            }
            
            int index = this.grid.ToIndex(x, y, z);

            // Check for valid index
            if (0 <= index && index < this.cellEntities.Length) {
                return ValueTypeOption<Entity>.Some(this.cellEntities[index].entity);
            }
            
            return ValueTypeOption<Entity>.None;
        }

        public ValueTypeOption<Entity> GetCellEntity(in GridCoord3 gridCoordinate) {
            return GetCellEntity(gridCoordinate.value.x, gridCoordinate.value.y, gridCoordinate.value.z);
        }
        
        public ValueTypeOption<Entity> GetCellEntity(in int3 gridCoordinate) {
            return GetCellEntity(gridCoordinate.x, gridCoordinate.y, gridCoordinate.z);
        }

        public int ToIndex(GridCoord3 gridCoordinate) {
            return this.grid.ToIndex(gridCoordinate);
        }
        
        public bool IsInside(in WorldCoord3 coordinate) {
            return this.grid.IsInsideAsWorld(coordinate.value.x, coordinate.value.y, coordinate.value.z);
        }

        public bool IsInside(in GridCoord3 coordinate) {
            return this.grid.IsInsideAsGrid(coordinate.value.x, coordinate.value.y, coordinate.value.z);
        }

        /// <summary>
        /// Transform from world position to grid coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public ValueTypeOption<GridCoord3> ToGridCoordinate(float3 worldPosition) {
            if (worldPosition.x < this.worldBoundingBox.Min.x) {
                // Outside of left side of bounds
                return ValueTypeOption<GridCoord3>.None;
            }
            
            if (worldPosition.x > this.worldBoundingBox.Max.x) {
                // Outside of right side bounds
                return ValueTypeOption<GridCoord3>.None;
            }
            
            if (worldPosition.y < this.worldBoundingBox.Min.y) {
                // Outside of bottom side of bounds
                return ValueTypeOption<GridCoord3>.None;
            }
            
            if (worldPosition.y > this.worldBoundingBox.Max.y) {
                // Outside of top side bounds
                return ValueTypeOption<GridCoord3>.None;
            }

            float xDiff = worldPosition.x - this.worldBoundingBox.Min.x;
            int xCoord = (int)(xDiff / this.grid.cellWidth);
            
            float yDiff = worldPosition.y - this.worldBoundingBox.Min.y;
            int yCoord = (int)(yDiff / this.grid.cellHeight);
            
            // Note that we don't determine the z here (the level)
            return ValueTypeOption<GridCoord3>.Some(new GridCoord3(xCoord, yCoord, 0));
        }
    }
}