using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// A struct wrapper to a 2D grid so we can pass it to jobs
    /// </summary>
    public struct GridWrapper {
        public readonly Grid2D grid;
        public readonly NativeArray<EntityBufferElement> cellEntities;

        public GridWrapper(Grid2D grid, NativeArray<EntityBufferElement> cellEntities) {
            this.grid = grid;
            this.cellEntities = cellEntities;
        }
        
        /// <summary>
        /// Returns the cell entity at the specified coordinate
        /// Expected coordinates is grid coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ValueTypeOption<Entity> GetCellEntity(int x, int y) {
            int index = y * this.grid.columnCount + x;

            if (index < 0 || index >= this.cellEntities.Length) {
                // Invalid index
                return ValueTypeOption<Entity>.None;
            }

            return ValueTypeOption<Entity>.Some(this.cellEntities[index].entity); 
        }

        public ValueTypeOption<Entity> GetCellEntity(int2 gridCoordinates) {
            return GetCellEntity(gridCoordinates.x, gridCoordinates.y);
        }

        public ValueTypeOption<Entity> GetCellEntityAtWorld(int worldX, int worldY) {
            return GetCellEntityAtWorld(new int2(worldX, worldY));
        }

        public ValueTypeOption<Entity> GetCellEntityAtWorld(int2 worldCoordinate) {
            // Convert to grid coordinate first
            int2 gridCoordinate = worldCoordinate - this.grid.minCoordinate;
            return GetCellEntity(gridCoordinate.x, gridCoordinate.y);
        }

        /// <summary>
        /// Transforms the specified world coordinate into a grid index
        /// </summary>
        /// <param name="worldCoordinate"></param>
        /// <returns></returns>
        public int ToIndex(int2 worldCoordinate) {
            int2 gridCoordinate = worldCoordinate - this.grid.minCoordinate;
            return gridCoordinate.y * this.grid.columnCount + gridCoordinate.x;
        }

        public bool IsInside(int2 worldCoordinate) {
            return this.grid.IsInsideGrid(worldCoordinate.x, worldCoordinate.y);
        }
    }
}