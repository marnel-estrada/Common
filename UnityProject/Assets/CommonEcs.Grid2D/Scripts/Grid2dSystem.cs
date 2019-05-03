using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Common;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class Grid2dSystem : ComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkComponentType<Grid2D> gridType;
        private ArchetypeChunkBufferType<EntityBufferElement> bufferType;

        private bool resolved;
        private Grid2D grid;
        private Maybe<NativeArray<EntityBufferElement>> cellEntities;

        private GridWrapper gridWrapper;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(Grid2D), ComponentType.Create<EntityBufferElement>());
        }

        protected override void OnDestroyManager() {
            if (this.cellEntities.HasValue) {
                this.cellEntities.Value.Dispose();
            }
        }

        protected override void OnUpdate() {
            if (this.resolved) {
                // Grid was already resolved
                return;
            }

            this.gridType = GetArchetypeChunkComponentType<Grid2D>(true);
            this.bufferType = GetArchetypeChunkBufferType<EntityBufferElement>(true);
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private void Process(in ArchetypeChunk chunk) {
            Assertion.Assert(chunk.Count > 0);
            NativeArray<Grid2D> grids = chunk.GetNativeArray(this.gridType);
            BufferAccessor<EntityBufferElement> buffers = chunk.GetBufferAccessor(this.bufferType);
            
            // Store only the first one
            this.grid = grids[0];
            PopulateCellEntities(buffers[0]); // We copy it because the DynamicBuffer gotten here will be disposed
            this.gridWrapper = new GridWrapper(this.grid, this.cellEntities.Value);
            this.resolved = true;
        }

        private void PopulateCellEntities(DynamicBuffer<EntityBufferElement> buffer) {
            this.cellEntities = new Maybe<NativeArray<EntityBufferElement>>(new NativeArray<EntityBufferElement>(buffer.Length, Allocator.Persistent));
            NativeArray<EntityBufferElement> array = this.cellEntities.Value;
            for (int i = 0; i < buffer.Length; ++i) {
                array[i] = buffer[i];
            }
        }

        /// <summary>
        /// Returns the cell entity at the specified coordinate
        /// Expected coordinates is grid coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Maybe<Entity> GetCellEntity(int x, int y) {
            Assertion.Assert(this.resolved);
            return this.gridWrapper.GetCellEntity(x, y);
        }
        
        public Maybe<Entity> GetCellEntityAtWorld(int worldX, int worldY) {
            return this.gridWrapper.GetCellEntityAtWorld(worldX, worldY);
        }

        public Maybe<Entity> GetCellEntityAtWorld(int2 worldCoordinate) {
            return this.gridWrapper.GetCellEntityAtWorld(worldCoordinate);
        }

        public bool IsInside(int2 coordinate) {
            return this.gridWrapper.IsInside(coordinate);
        }
        
        public ref readonly GridWrapper GridWrapper {
            get {
                return ref this.gridWrapper;
            }
        }

        public ref readonly Grid2D Grid2D {
            get {
                return ref this.grid;
            }
        }
    }
}