using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public readonly struct MultipleGrid2dWrapper {
        public readonly MultipleGrid2D grid;
        
        // Note that this array already contains all tiles for all levels
        public readonly NativeArray<EntityBufferElement> cellEntities;

        public MultipleGrid2dWrapper(MultipleGrid2D grid, NativeArray<EntityBufferElement> cellEntities) {
            this.grid = grid;
            this.cellEntities = cellEntities;
        }

        public ValueTypeOption<Entity> GetCellEntity(int x, int y, int z) {
            int index = this.grid.ToIndex(x, y, z);

            // Check for valid index
            if (0 <= index && index < this.cellEntities.Length) {
                return ValueTypeOption<Entity>.Some(this.cellEntities[index].entity);
            }
            
            return ValueTypeOption<Entity>.NONE;
        }

        public ValueTypeOption<Entity> GetCellEntity(int3 gridCoordinate) {
            return GetCellEntity(gridCoordinate.x, gridCoordinate.y, gridCoordinate.z);
        }

        public int ToIndex(int3 gridCoordinate) {
            return this.grid.ToIndex(gridCoordinate);
        }
    }
}