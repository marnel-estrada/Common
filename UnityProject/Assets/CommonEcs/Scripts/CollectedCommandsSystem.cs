using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CollectedCommandsSystem : ComponentSystem {
        private EntityCommandBuffer? pendingBuffer;

        protected override void OnUpdate() {
            if (this.pendingBuffer != null) {
                this.pendingBuffer.Value.Playback(this.EntityManager);
                this.pendingBuffer.Value.Dispose();
                this.pendingBuffer = null;
            }
        }
        
        public EntityCommandBuffer Buffer {
            get {
                if (this.pendingBuffer == null) {
                    this.pendingBuffer = new EntityCommandBuffer(Allocator.TempJob);
                }

                return this.pendingBuffer.Value;
            }
        }
    }
}