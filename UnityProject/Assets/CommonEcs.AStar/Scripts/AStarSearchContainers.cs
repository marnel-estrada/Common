using System;

using Unity.Collections;
using Unity.Mathematics;

namespace CommonEcs {
    public readonly struct AStarSearchContainers : IDisposable {
        public readonly NativeList<AStarNode> allNodes;

        // These two is used for the OpenSet
        public readonly NativeList<HeapNode> heapNodes;
        public readonly NativeHashMap<int2, AStarNode> nodeMap;
        
        public readonly NativeHashMap<int2, byte> closeSet;

        public AStarSearchContainers(NativeList<AStarNode> allNodes,
            NativeList<HeapNode> heapNodes,
            NativeHashMap<int2, AStarNode> nodeMap,
            NativeHashMap<int2, byte> closeSet) {
            this.allNodes = allNodes;
            this.heapNodes = heapNodes;
            this.nodeMap = nodeMap;
            this.closeSet = closeSet;
        }

        public void Dispose() {
            this.allNodes.Dispose();
            this.heapNodes.Dispose();
            this.nodeMap.Dispose();
            this.closeSet.Dispose();
        }
    }
}