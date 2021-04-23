using System;

namespace CommonEcs {
    public struct HeapNode<T> where T : unmanaged, IEquatable<T> {
        public readonly AStarNode<T> value;
        public float cost;
        public int next;
        
        public HeapNode(AStarNode<T> value, float cost) {
            this.value = value;
            this.cost = cost;
            this.next = -1;
        }
    }
}