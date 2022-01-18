using System;

namespace CommonEcs {
    public readonly struct AStarNode<T> where T : unmanaged, IEquatable<T> {
        public readonly int index;
        public readonly T position;
        public readonly int parent;
        
        public readonly float G;
        public readonly float H;
        public readonly float F;

        public AStarNode(int index, T position, int parent, float g, float h) {
            this.index = index;
            this.position = position;
            this.parent = parent;
            this.G = g;
            this.H = h;
            this.F = g + h;
        }
    }
}