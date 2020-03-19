using Unity.Entities;

namespace CommonEcs {
    public struct Waiting : IComponentData {
        public bool done; // Done here means done waiting
        public bool special;
    }
}