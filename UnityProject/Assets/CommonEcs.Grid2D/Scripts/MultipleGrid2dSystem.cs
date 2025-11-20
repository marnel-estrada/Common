using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    [UpdateBefore(typeof(ScalableTimeSystemGroup))]
    public partial class MultipleGrid2dSystem : SystemBase {
        private bool resolved;
        private MultipleGrid2D grid;
        private Maybe<NativeArray<EntityBufferElement>> cellEntities;

        private MultipleGrid2dWrapper gridWrapper;

        private EntityQuery query;

        protected override void OnCreate() {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<MultipleGrid2D>()
                .WithAll<EntityBufferElement>()
                .Build(this);
        }

        private ComponentTypeHandle<MultipleGrid2D> multipleGrid2dType;
        private BufferTypeHandle<EntityBufferElement> entityBufferType;
        private ComponentLookup<Cell2D> allCells;

        protected override void OnUpdate() {
            this.multipleGrid2dType = GetComponentTypeHandle<MultipleGrid2D>(true);
            this.entityBufferType = GetBufferTypeHandle<EntityBufferElement>(true);
            this.allCells = GetComponentLookup<Cell2D>();

            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.Temp);
            for (int i = 0; i < chunks.Length; i++) {
                ArchetypeChunk chunk = chunks[i];
                ProcessChunk(ref chunk);
            }
        }

        private void ProcessChunk(ref ArchetypeChunk chunk) {
            NativeArray<MultipleGrid2D> grids = chunk.GetNativeArray(ref this.multipleGrid2dType);
            BufferAccessor<EntityBufferElement> entityBufferElementsBuffers = chunk.GetBufferAccessor(ref this.entityBufferType);
            
            for (int i = 0; i < chunk.Count; i++) {
                if (this.resolved) {
                    // Resolve only once
                    return;
                }
                
                MultipleGrid2D multipleGrid = grids[i];
                DynamicBuffer<EntityBufferElement> entityBuffer = entityBufferElementsBuffers[i];
                
                this.grid = multipleGrid;
                PopulateCellEntities(in entityBuffer);
                
                // Prepare the bounding box
                float2 min = allCells[this.cellEntities.Value[0].entity].BottomLeft; // first cell

                int cellCount = this.cellEntities.Value.Length;
                float2 max = allCells[this.cellEntities.Value[cellCount - 1].entity].TopRight;
                Aabb2 worldBoundingBox = new(min, max);
                
                this.gridWrapper = new MultipleGrid2dWrapper(this.grid, this.cellEntities.Value, worldBoundingBox);

                this.resolved = true;
                this.Enabled = false; // So update will not be called again
            }
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
        
        public ValueTypeOption<Entity> GetCellEntity(in GridCoord3 gridCoordinate) {
            Assertion.IsTrue(this.resolved);
            return this.gridWrapper.GetCellEntity(gridCoordinate.value);
        }
        
        public ValueTypeOption<Entity> GetCellEntity(int3 gridCoordinate) {
            Assertion.IsTrue(this.resolved);
            return this.gridWrapper.GetCellEntity(gridCoordinate.x, gridCoordinate.y, gridCoordinate.z);
        }
        
        public ref readonly MultipleGrid2dWrapper GridWrapper => ref this.gridWrapper;

        public ref readonly MultipleGrid2D Grid2D => ref this.grid;

        /// <summary>
        /// Returns whether or not the grid is resolved
        /// </summary>
        public bool Resolved => this.resolved;
    }
}