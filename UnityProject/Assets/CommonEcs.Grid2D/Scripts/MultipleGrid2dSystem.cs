using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public class MultipleGrid2dSystem : SystemBase {
        private bool resolved;
        private Grid2D grid;
        private Maybe<NativeArray<EntityBufferElement>> cellEntities;

        private MultipleGrid2dWrapper gridWrapper;
        
        protected override void OnUpdate() {
            
        }
    }
}