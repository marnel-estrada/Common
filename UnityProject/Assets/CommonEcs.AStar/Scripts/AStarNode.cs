using Unity.Mathematics;

namespace CommonEcs {
    public readonly struct AStarNode {
        public readonly int index;
        public readonly int2 position;
        public readonly int parent;
        
        public readonly float G;
        public readonly float H;
        public readonly float F;

        public AStarNode(int index, int2 position, int parent, float g, float h) {
            this.index = index;
            this.position = position;
            this.parent = parent;
            this.G = g;
            this.H = h;
            this.F = g + h;
        }
    }
}