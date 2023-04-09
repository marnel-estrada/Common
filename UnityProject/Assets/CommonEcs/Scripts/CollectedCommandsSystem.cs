using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// We don't add this in PresentationSystemGroup so that new entities would be processed by the
    /// transform system after playback of the buffer
    /// </summary>
    public partial class CollectedCommandsSystem : SystemBase {
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