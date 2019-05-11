using Unity.Entities;

namespace CommonEcs {
    public struct AStarPath : IComponentData {
        public ByteBool reachable;
        
        private int currentIndex;
        private int positionCount;

        public AStarPath(int positionCount, bool reachable) {
            this.positionCount = positionCount;
            this.currentIndex = -1;

            this.reachable = reachable;
        }

        public void Clear() {
            this.reachable = false;
            this.positionCount = 0;
            this.currentIndex = -1;
        }
    }
}