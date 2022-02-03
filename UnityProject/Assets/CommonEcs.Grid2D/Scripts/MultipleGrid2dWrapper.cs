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
            if (x < 0 || y < 0 || z < this.grid.minGridCoordinate.value.z) {
                // Outside of map
                return ValueTypeOption<Entity>.None;
            }

            if (x >= this.grid.columns || y >= this.grid.rows || z > this.grid.maxGridCoordinate.value.z) {
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
        /// Transform from world position to grid coordinate.
        /// Note that the z coordinate of the world position matters now as it denotes which floor level
        /// it is.
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

            // Note here that positive z means negative z in world space so that the sprite would be closer 
            // to the camera.
            int zCoord = (int)math.round(worldPosition.z / -this.grid.cellHeight);
            
            return ValueTypeOption<GridCoord3>.Some(new GridCoord3(xCoord, yCoord, zCoord));
        }

        public ValueTypeOption<Entity> GetCellEntityFromWorld(in float3 worldPosition) {
            ValueTypeOption<GridCoord3> gridCoordinate = ToGridCoordinate(worldPosition);
            if (gridCoordinate.IsNone) {
                // Must be outside map
                return ValueTypeOption<Entity>.None;
            }

            return GetCellEntity(gridCoordinate.ValueOrError());
        }

        // Note here that the z position is just multiplied with cell height
        // It's basically the same distance when moving from cell to cell in XY.
        // We multiply by negative here so that the sprite will move closer to the camera instead
        // of going farther.
        public float ToWorldZPosition(int zCoordinate) {
            return zCoordinate * -this.grid.cellHeight;
        }
    }
}