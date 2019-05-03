using Unity.Collections;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// A wrapper for the open set
    /// </summary>
    public struct OpenSet {
        private GrowingHeap heap;
        private NativeHashMap<int2, AStarNode> map;

        public OpenSet(GrowingHeap heap, NativeHashMap<int2, AStarNode> map) {
            this.heap = heap;
            this.map = map;
        }

        public void Clear() {
            this.heap.Clear();
            this.map.Clear();
        }

        public void Push(AStarNode node) {
            this.heap.Push(node);
            this.map.AddOrReplace<int2, AStarNode>(node.position, node);
        }

        public AStarNode Pop() {
            AStarNode result = this.heap.Pop();
            this.map.Remove(result.position);

            return result;
        }

        public bool TryGet(int2 position, out AStarNode node) {
            return this.map.TryGetValue(position, out node);
        }

        public bool HasItems {
            get {
                return this.heap.HasItems;
            }
        }
    }
}