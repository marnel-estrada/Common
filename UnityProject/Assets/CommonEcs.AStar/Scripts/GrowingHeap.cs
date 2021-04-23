using System;

using Unity.Collections;

namespace CommonEcs {
    public struct GrowingHeap<T> where T : unmanaged, IEquatable<T> {
        private int head;
        private int count;

        public NativeList<HeapNode<T>> nodes;

        public GrowingHeap(NativeList<HeapNode<T>> nodes) {
            this.nodes = nodes;
            this.head = -1;
            this.count = 0;
        }
        
        public bool HasItems {
            get {
                return this.head >= 0;
            }
        }
        
        public void Push(AStarNode<T> value) {
            HeapNode<T> node = new HeapNode<T>(value, value.F);
            
            if (this.head < 0) {
                // No entries yet
                this.head = this.nodes.Length;
            } else if (node.cost < this.nodes[this.head].cost) {
                // New node has lesser cost than the current head element
                node.next = this.head;
                this.head = this.nodes.Length;
            } else {
                // Look for the correct position of the new node
                int currentIndex = this.head;
                HeapNode<T> current = this.nodes[currentIndex];

                // Keep going until we find a position such that node.cost < current
                while (current.next >= 0 && this.nodes[current.next].cost <= node.cost) {
                    currentIndex = current.next;
                    current = this.nodes[currentIndex];
                }

                node.next = current.next;
                current.next = this.nodes.Length;

                this.nodes[currentIndex] = current; // Modify the current node since we changed its next value
            }

            this.nodes.Add(node);
            ++this.count;
        }
        
        public AStarNode<T> Pop() {
            AStarNode<T> result = this.Top;
            this.head = this.nodes[this.head].next;
            
            // Note here that we don't decrement length
            // length is always used as the index where new elements are pushed
            // I think it was done this way so that there's no need to maintain a list of indices with no element in it
            // The array just keeps growing even if we pop
            --this.count;

            return result;
        }
        
        public AStarNode<T> Top {
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
                return this.nodes.Length;
            }
        }

        public int ElementCount {
            get {
                return this.count;
            }
        }

        public HeapNode<T> this[int index] {
            get {
                return this.nodes[index];
            }
        }

        public void Clear() {
            this.head = -1;
            this.nodes.Clear();
            this.count = 0;
        }
    }
}