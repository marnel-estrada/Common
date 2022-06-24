using System;

using Unity.Collections;

namespace CommonEcs {
    /// <summary>
    /// A wrapper for the open set
    /// </summary>
    public struct OpenSet<T> where T : unmanaged, IEquatable<T> {
        private GrowingHeap<T> heap;
        private NativeParallelHashMap<T, AStarNode<T>> map;

        public OpenSet(GrowingHeap<T> heap, NativeParallelHashMap<T, AStarNode<T>> map) {
            this.heap = heap;
            this.map = map;
        }

        public void Clear() {
            this.heap.Clear();
            this.map.Clear();
        }

        public void Push(AStarNode<T> node) {
            this.heap.Push(node);
            this.map.AddOrReplace<T, AStarNode<T>>(node.position, node);
        }

        public AStarNode<T> Pop() {
            AStarNode<T> result = this.heap.Pop();
            this.map.Remove(result.position);

            return result;
        }

        public bool TryGet(T position, out AStarNode<T> node) {
            return this.map.TryGetValue(position, out node);
        }

        public bool HasItems {
            get {
                return this.heap.HasItems;
            }
        }
    }
}