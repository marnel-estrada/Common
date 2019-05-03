namespace CommonEcs {
    public struct HeapNode {
        public readonly AStarNode value;
        public float cost;
        public int next;
        
        public HeapNode(AStarNode value, float cost) {
            this.value = value;
            this.cost = cost;
            this.next = -1;
        }
    }
}