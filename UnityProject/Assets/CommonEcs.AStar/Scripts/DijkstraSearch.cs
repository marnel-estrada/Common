using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [BurstCompile]
    public struct DijkstraSearch<GoalIdentifierType, ReachabilityType> : IJob 
        where GoalIdentifierType : struct, GoalIdentifier 
        where ReachabilityType : struct, Reachability {
        public Entity owner;
        public int2 startPosition; 

        [ReadOnly]
        public GoalIdentifierType goalIdentifier;
        
        [ReadOnly]
        public ReachabilityType reachability;
        
        [ReadOnly]
        public NativeArray<int2> neighborOffsets; // This will be specified by client on whether it wants to include diagonal neighbors

        public GridWrapper gridWrapper;
        
        public ComponentDataFromEntity<AStarPath> allPaths;
        
        [ReadOnly]
        public BufferFromEntity<Int2BufferElement> allPathLists;

        public ComponentDataFromEntity<Waiting> allWaiting;

        // This is the master container for all AStarNodes. The key is the hash code of the position.
        // This will be specified by client code
        public NativeList<AStarNode> allNodes;

        // Only used for existence of position in closed set
        // This will be specified by client code
        public NativeHashMap<int2, byte> closeSet;

        public OpenSet openSet;

        public void Execute() {
            this.allNodes.Clear();
            this.openSet.Clear();
            this.closeSet.Clear();
            
            DoSearch();
            
            // Mark as done waiting for the agent to respond
            this.allWaiting[this.owner] = new Waiting { done = true };
        }

        private void DoSearch() {
            AStarNode startNode = CreateNode(this.startPosition, -1, 0, 0);
            this.openSet.Push(startNode);
            
            // Process while there are nodes in the open set
            while (this.openSet.HasItems) {
                AStarNode current = this.openSet.Pop();
                if (this.goalIdentifier.IsGoal(current.position)) {
                    // Goal has been found
                    int pathCount = ConstructPath(current);
                    this.allPaths[this.owner] = new AStarPath(pathCount, true);

                    return;
                }

                ProcessNode(current);

                this.closeSet.TryAdd(current.position, 0);
            }
            
            // Mark as unreachable
            MarkUnreachable();
        }

        private void MarkUnreachable() {
            AStarPath path = this.allPaths[this.owner];
            path.Clear();
            path.reachable = false;
            this.allPaths[this.owner] = path;

            DynamicBuffer<Int2BufferElement> pathList = this.allPathLists[this.owner];
            pathList.Clear();
        }

        private AStarNode GetNode(int index) {
            return this.allNodes[index];
        }

        private AStarNode CreateNode(int2 position, int parent, float g, float h) {
            int index = this.allNodes.Length;
            AStarNode node = new AStarNode(index, position, parent, g, h);
            this.allNodes.Add(node);

            return node;
        }
        
        // Returns the position count in the path
        private int ConstructPath(AStarNode destination) {
            // Note here that we no longer need to reverse the ordering of the path
            // We just add them as reversed in AStarPath
            // AStarPath then knows how to handle this
            DynamicBuffer<Int2BufferElement> pathList = this.allPathLists[this.owner];
            pathList.Clear();
            AStarNode current = GetNode(destination.index);
            while (current.parent >= 0) {
                pathList.Add(new Int2BufferElement(current.position));
                current = GetNode(current.parent);
            }

            return pathList.Length;
        }
        
        private void ProcessNode(in AStarNode current) {
            if (IsInCloseSet(current.position)) {
                // Already in closed set. We no longer process because the same node with lower F
                // might have already been processed before. Note that we don't fix the heap. We just
                // keep on pushing nodes with lower scores.
                return;
            }
        
            // Process neighbors
            for (int i = 0; i < this.neighborOffsets.Length; ++i) {
                int2 neighborPosition = current.position + this.neighborOffsets[i];
                
                if (current.position.Equals(neighborPosition)) {
                    // No need to process if they are equal
                    continue;
                }
        
                if (!this.gridWrapper.IsInside(neighborPosition)) {
                    // No longer inside the map
                    continue;
                }
            
                if (IsInCloseSet(neighborPosition)) {
                    // Already in close set
                    continue;
                }
        
                if (!this.reachability.IsReachable(current.position, neighborPosition)) {
                    // Not reachable based from specified reachability
                    continue;
                }
                
                float tentativeG = current.G + this.reachability.GetWeight(current.position, neighborPosition);

                if (this.openSet.TryGet(neighborPosition, out AStarNode existingNode)) {
                    // This means that the node is already in the open set
                    // We update the node if the current movement is better than the one in the open set
                    if (tentativeG < existingNode.G) {
                        // Found a better path. Replace the values.
                        // Note that creation automatically replaces the node at that position
                        AStarNode betterNode = CreateNode(neighborPosition, current.index, tentativeG, 0);
                        
                        // Only add to open set if it's a better movement
                        // If we just push without checking, a node with the same g score will be pushed
                        // which causes infinite loop as every node will be pushed
                        this.openSet.Push(betterNode);
                    }
                } else {   
                    AStarNode neighborNode = CreateNode(neighborPosition, current.index, tentativeG, 0);
                    this.openSet.Push(neighborNode);
                }
            }
        }

        private bool IsInCloseSet(int2 position) {
            return this.closeSet.TryGetValue(position, out _);
        }
    }
}