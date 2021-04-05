using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    public struct DebugComponent : IComponentData {
        public FixedString32 id;
        public ByteBool isDebug;

        public DebugComponent(FixedString32 id, ByteBool isDebug) {
            this.id = id;
            this.isDebug = isDebug;
        }
        
        public DebugComponent(FixedString32 id) : this(id, false) {
        }
    }
}
