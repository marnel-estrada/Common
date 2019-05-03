using Common;

using Unity.Entities;

namespace CommonEcs {
    public struct Waiting : IComponentData {
        public ByteBool done; // Done here means done waiting
    }
}