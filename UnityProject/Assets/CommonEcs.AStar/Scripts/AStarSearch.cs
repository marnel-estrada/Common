using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    //[BurstCompile]
    public struct AStarSearch<HeuristicCalculator, ReachabilityType> : IJob
        where HeuristicCalculator : struct, HeuristicCostCalculator where ReachabilityType : struct, Reachability {
        public Entity owner;
        public int2 startPosition;
        public int2 goalPosition;

        [ReadOnly]
        public ReachabilityType reachability;

        // This will be specified by client on whether it wants to include diagonal neighbors
        [ReadOnly]
        public NativeArray<int2> neighborOffsets;

        [ReadOnly]
        public GridWrapper gridWrapper;

        public ComponentDataFromEntity<AStarPath> allPaths;

        public BufferFromEntity<Int2BufferElement> allPathLists;

        public ComponentDataFromEntity<Waiting> allWaiting;

        // This is the master container for all AStarNodes. The key is the hash code of the position.
        // This will be specified by client code
        public NativeList<AStarNode> allNodes;

        // Only used for existence of position in closed set
        // This will be specified by client code
        public NativeHashMap<int2, byte> closeSet;

        [ReadOnly]
        private HeuristicCalculator heuristicCalculator;

        public OpenSet openSet;

        public void Execute() {
            this.allNodes.Clear();
            this.openSet.Clear();
            this.closeSet.Clear();

            DoSearch();

            Waiting waiting = this.allWaiting[this.owner];
            
            // Mark as done waiting for the agent to respond
            this.allWaiting[this.owner] = new Waiting {
                done = true,
                special = waiting.special
            };
        }

        private void DoSearch() {
            if (!this.reachability.IsReachable(0, this.goalPosition)) {
                // Goal is not reachable
                this.allPaths[this.owner] = new AStarPath(0, false);

                return;
            }

            float startNodeH = this.heuristicCalculator.ComputeCost(this.startPosition, this.goalPosition);
            AStarNode startNode = CreateNode(this.startPosition, -1, 0, startNodeH);
            this.openSet.Push(startNode);

            float minH = float.MaxValue;
            Maybe<AStarNode> minHPosition = Maybe<AStarNode>.Nothing;

            // Process while there are nodes in the open set
            while (this.openSet.HasItems) {
                AStarNode current = this.openSet.Pop();
                if (current.position.Equals(this.goalPosition)) {
                    // Goal has been found
                    int pathCount = ConstructPath(current);
                    this.allPaths[this.owner] = new AStarPath(pathCount, true);

                    return;
                }

                ProcessNode(current);

                this.closeSet.TryAdd(current.position, 0);

                // We save the node with the least H so we could still try to locate
                // the nearest position to the destination 
                if (current.H < minH) {
                    minHPosition = new Maybe<AStarNode>(current);
                    minH = current.H;
                }
            }

            // Open set has been exhausted. Path is unreachable.
            if (minHPosition.HasValue) {
                int pathCount = ConstructPath(minHPosition.Value);
                this.allPaths[this.owner] = new AStarPath(pathCount, false); // false for unreachable
            } else {
                this.allPaths[this.owner] = new AStarPath(0, false);
            }
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
            AStarNode current = this.allNodes[destination.index];
            while (current.parent >= 0) {
                pathList.Add(new Int2BufferElement(current.position));
                current = this.allNodes[current.parent];
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

                if (!this.reachability.IsReachable(0, current.position, neighborPosition)) {
                    // Not reachable based from specified reachability
                    continue;
                }

                float tentativeG = current.G + this.reachability.GetWeight(0, current.position, neighborPosition);

                float h = this.heuristicCalculator.ComputeCost(neighborPosition, this.goalPosition);

                if (this.openSet.TryGet(neighborPosition, out AStarNode existingNode)) {
                    // This means that the node is already in the open set
                    // We update the node if the current movement is better than the one in the open set
                    if (tentativeG < existingNode.G) {
                        // Found a better path. Replace the values.
                        // Note that creation automatically replaces the node at that position
                        AStarNode betterNode = CreateNode(neighborPosition, current.index, tentativeG, h);

                        // Only add to open set if it's a better movement
                        // If we just push without checking, a node with the same g score will be pushed
                        // which causes infinite loop as every node will be pushed
                        this.openSet.Push(betterNode);
                    }
                } else {
                    AStarNode neighborNode = CreateNode(neighborPosition, current.index, tentativeG, h);
                    this.openSet.Push(neighborNode);
                }
            }
        }

        private bool IsInCloseSet(int2 position) {
            return this.closeSet.TryGetValue(position, out _);
        }
    }
}