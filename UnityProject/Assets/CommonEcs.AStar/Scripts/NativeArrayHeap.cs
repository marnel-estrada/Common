using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// It's a heap that wraps a NativeArray.
    /// </summary>
    public struct NativeArrayHeap {
        private int head;
        private int length;
        private int count;

        private NativeArray<HeapNode> nodes;

        public NativeArrayHeap(NativeArray<HeapNode> nodes) {
            this.nodes = nodes;
            this.head = -1;
            this.length = 0;
            this.count = 0;
        }

        public bool HasItems {
            get {
                return this.head >= 0;
            }
        }

        public void Push(AStarNode value) {
            HeapNode node = new HeapNode(value, value.F);
            
            if (this.head < 0) {
                // No entries yet
                this.head = this.length;
            } else if (node.cost < this.nodes[this.head].cost) {
                // New node has lesser cost than the current head element
                node.next = this.head;
                this.head = this.length;
            } else {
                // Look for the correct position of the new node
                int currentIndex = this.head;
                HeapNode current = this.nodes[currentIndex];

                // Keep going until we find a position such that node.cost < current
                while (current.next >= 0 && this.nodes[current.next].cost <= node.cost) {
                    currentIndex = current.next;
                    current = this.nodes[currentIndex];
                }

                node.next = current.next;
                current.next = this.length;

                this.nodes[currentIndex] = current; // Modify the current node since we changed its next value
            }

            this.nodes[this.length] = node;
            ++this.length;
            ++this.count;
        }

        public AStarNode Pop() {
            AStarNode result = this.Top;
            this.head = this.nodes[this.head].next;
            
            // Note here that we don't decrement length
            // length is always used as the index where new elements are pushed
            // I think it was done this way so that there's no need to maintain a list of indices with no element in it
            // The array just keeps growing even if we pop
            --this.count;

            return result;
        }

        public AStarNode Top {
            get {
                return this.nodes[this.head].value;
            }
        }

        // Note that this is different from ElementCount
        // Note that array positions are still blocked during popping
        // This is the actual length of the array being used
        // Use this value to iterate through the values of the array
        public int Length {
            get {
                return this.length;
            }
        }

        public int ElementCount {
            get {
                return this.count;
            }
        }

        public HeapNode this[int index] {
            get {
                return this.nodes[index];
            }
        }

        public void Clear() {
            this.head = -1;
            this.length = 0;
            this.count = 0;
        }
    }
}