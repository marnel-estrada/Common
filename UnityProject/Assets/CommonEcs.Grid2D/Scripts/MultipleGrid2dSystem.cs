using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public class MultipleGrid2dSystem : SystemBase {
        private bool resolved;
        private MultipleGrid2D grid;
        private Maybe<NativeArray<EntityBufferElement>> cellEntities;

        private MultipleGrid2dWrapper gridWrapper;
        
        protected override void OnUpdate() {
            ComponentDataFromEntity<Cell2D> allCells = GetComponentDataFromEntity<Cell2D>();
            
            this.Entities.ForEach(
                delegate(in MultipleGrid2D multipleGrid, in DynamicBuffer<EntityBufferElement> entityBuffer) {
                    if (this.resolved) {
                        // Resolve only once
                        return;
                    }
                    
                    this.grid = multipleGrid;
                    PopulateCellEntities(in entityBuffer);
                    
                    // Prepare the bounding box
                    float2 min = allCells[this.cellEntities.Value[0].entity].BottomLeft; // first cell

                    int cellCount = this.cellEntities.Value.Length;
                    float2 max = allCells[this.cellEntities.Value[cellCount - 1].entity].TopRight;
                    Aabb2 worldBoundingBox = new Aabb2(min, max);
                    
                    this.gridWrapper = new MultipleGrid2dWrapper(this.grid, this.cellEntities.Value, worldBoundingBox);

                    this.resolved = true;
                    this.Enabled = false; // So update will not be called again
                }).WithoutBurst().Run();
        }
        
        private void PopulateCellEntities(in DynamicBuffer<EntityBufferElement> entityBuffer) {
            this.cellEntities = new Maybe<NativeArray<EntityBufferElement>>(new NativeArray<EntityBufferElement>(entityBuffer.Length, Allocator.Persistent));
            NativeArray<EntityBufferElement> array = this.cellEntities.Value;
            for (int i = 0; i < entityBuffer.Length; ++i) {
                array[i] = entityBuffer[i];
            }
        }
        
        protected override void OnDestroy() {
            if (this.cellEntities.HasValue) {
                this.cellEntities.Value.Dispose();
            }
        }
        
        /// <summary>
        /// Returns the cell entity at the specified coordinate
        /// Expected coordinates is grid coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ValueTypeOption<Entity> GetCellEntity(int x, int y, int z) {
            Assertion.IsTrue(this.resolved);
            return this.gridWrapper.GetCellEntity(x, y, z);
        }
        
        public ValueTypeOption<Entity> GetCellEntity(int3 gridCoordinate) {
            Assertion.IsTrue(this.resolved);
            return this.gridWrapper.GetCellEntity(gridCoordinate.x, gridCoordinate.y, gridCoordinate.z);
        }
        
        public ref readonly MultipleGrid2dWrapper GridWrapper {
            get {
                return ref this.gridWrapper;
            }
        }

        public ref readonly MultipleGrid2D Grid2D {
            get {
                return ref this.grid;
            }
        }

        /// <summary>
        /// Returns whether or not the grid is resolved
        /// </summary>
        public bool Resolved {
            get {
                return this.resolved;
            }
        }
    }
}