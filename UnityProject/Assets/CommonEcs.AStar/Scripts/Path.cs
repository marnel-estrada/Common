using Unity.Entities;

namespace CommonEcs {
    public struct Path : IComponentData {
        private int currentIndex;
        public bool reachable;

        public Path(bool reachable) {
            this.currentIndex = -1;
            this.reachable = reachable;
        }

        public void Clear() {
            this.reachable = false;
            this.currentIndex = -1;
        }
    }
}