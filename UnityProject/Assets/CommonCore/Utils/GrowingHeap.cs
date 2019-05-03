namespace Common {
    public class GrowingHeap<T> {
        private int head = -1;
        private int count;
        
        private readonly SimpleList<Node<T>> nodes;

        public GrowingHeap(int initialCapacity) {
            this.nodes = new SimpleList<Node<T>>(initialCapacity);
        }
        
        public bool HasItems {
            get {
                return this.head >= 0;
            }
        }

        public void Push(T item, float itemCost) {
            Node<T> newNode = new Node<T>(item, itemCost);
            
            if (this.head < 0) {
                // No entries yet
                this.head = this.nodes.Count;
            } else if (itemCost < this.nodes[this.head].cost) {
                // New node has lesser cost than the current head element
                newNode.next = this.head;
                this.head = this.nodes.Count;
            } else {
                // Look for the correct position of the new node
                int currentIndex = this.head;
                Node<T> current = this.nodes[currentIndex];
                
                // Keep going until we find a position such that cost < current.cost
                while (current.next >= 0 && this.nodes[current.next].cost <= itemCost) {
                    currentIndex = current.next;
                    current = this.nodes[currentIndex];
                }

                newNode.next = current.next;
                current.next = this.nodes.Count;

                // Modify the current node since we changed its next value
                this.nodes[currentIndex] = current; 
            }
            
            this.nodes.Add(newNode);
            ++this.count;
        }

        public T Pop() {
            T result = this.Top;
            this.head = this.nodes[this.head].next;

            // Note here that we don't remove from the list
            // The count of the list is always used as the index where new elements are pushed
            // I think it was done this way so that there's no need to maintain a list of indices with no element in it
            // The list just keeps growing even if we pop
            --this.count;

            return result;
        }
        
        public T Top {
            get {
                return this.nodes[this.head].item;
            }
        }

        public int Count {
            get {
                return this.count;
            }
        }

        public void Clear() {
            this.head = -1;
            this.nodes.Clear();
            this.count = 0;
        }
        
        public bool CheckIntegrity() {
            for (int i = 0; i < this.nodes.Count; ++i) {
                Node<T> node = this.nodes[i];
                if (node.next == i) {
                    // This causes infinite loop
                    // It must be avoided
                    return false;
                }
            }
            
            return true;
        }

        private struct Node<T> {
            public readonly T item;
            public readonly float cost;
            public int next;

            public Node(T item, float cost) {
                this.item = item;
                this.cost = cost;
                this.next = -1;
            }
        }
    }
}