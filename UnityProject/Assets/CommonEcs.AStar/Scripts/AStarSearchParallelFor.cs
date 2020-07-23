using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [BurstCompile]
    public struct AStarSearchParallelFor<HeuristicCalculator, ReachabilityType> : IJobParallelFor
        where HeuristicCalculator : struct, HeuristicCostCalculator where ReachabilityType : struct, Reachability {
        [ReadOnly]
        public NativeArray<Entity> entities; // The request entities

        [NativeDisableParallelForRestriction, ReadOnly]
        public ComponentDataFromEntity<AStarSearchParameters> allParameters;

        [NativeDisableParallelForRestriction, WriteOnly]
        public ComponentDataFromEntity<AStarPath> allPaths;

        [NativeDisableParallelForRestriction, WriteOnly]
        public ComponentDataFromEntity<Waiting> allWaiting;

        [NativeDisableParallelForRestriction, WriteOnly]
        public BufferFromEntity<Int2BufferElement> allPathLists;

        [ReadOnly]
        public ReachabilityType reachability;

        private HeuristicCalculator heuristicCalculator;

        // This will be specified by client on whether it wants to include diagonal neighbors
        [ReadOnly]
        public NativeArray<int2> neighborOffsets;

        [ReadOnly]
        public GridWrapper gridWrapper;

        // Execute search per entity in entities
        public void Execute(int index) {
            Search search = new Search() {
                index = index,
                entity = this.entities[index],
                allParameters = this.allParameters,
                allPaths = this.allPaths,
                allPathLists = this.allPathLists,
                reachability = this.reachability,
                heuristicCalculator = this.heuristicCalculator,
                neighborOffsets = this.neighborOffsets,
                gridWrapper = this.gridWrapper
            };
            search.Execute();

            this.allWaiting[this.entities[index]] = new Waiting() {
                done = true
            };
        }

        private struct Search {
            public int index;
            public Entity entity;

            public ComponentDataFromEntity<AStarSearchParameters> allParameters;
            public ComponentDataFromEntity<AStarPath> allPaths;
            public BufferFromEntity<Int2BufferElement> allPathLists;

            public ReachabilityType reachability;
            public HeuristicCalculator heuristicCalculator;
            public NativeArray<int2> neighborOffsets;

            public GridWrapper gridWrapper;

            // This is the master container for all AStarNodes. The key is the hash code of the position.
            // This will be specified by client code
            private NativeList<AStarNode> allNodes;

            private OpenSet openSet;

            // Only used for existence of position in closed set
            // This will be specified by client code
            private NativeHashMap<int2, byte> closeSet;

            private int2 goalPosition;

            public void Execute() {
                // Instantiate containers
                this.allNodes = new NativeList<AStarNode>(10, Allocator.Temp);
                
                NativeList<HeapNode> heapList = new NativeList<HeapNode>(10, Allocator.Temp);
                GrowingHeap heap = new GrowingHeap(heapList);

                NativeHashMap<int2,AStarNode> openSetMap = new NativeHashMap<int2, AStarNode>(10, Allocator.Temp);
                this.openSet = new OpenSet(heap, openSetMap);
                
                this.closeSet = new NativeHashMap<int2, byte>(10, Allocator.Temp);

                AStarSearchParameters parameters = this.allParameters[this.entity];
                this.goalPosition = parameters.goal;

                float startNodeH = this.heuristicCalculator.ComputeCost(parameters.start, this.goalPosition);
                AStarNode startNode = CreateNode(parameters.start, -1, 0, startNodeH);
                this.openSet.Push(startNode);

                float minH = float.MaxValue;
                Maybe<AStarNode> minHPosition = Maybe<AStarNode>.Nothing;

                // Process while there are nodes in the open set
                while (this.openSet.HasItems) {
                    AStarNode current = this.openSet.Pop();
                    if (current.position.Equals(this.goalPosition)) {
                        // Goal has been found
                        int pathCount = ConstructPath(current);
                        this.allPaths[this.entity] = new AStarPath(pathCount, true);

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
                    this.allPaths[this.entity] = new AStarPath(pathCount, false); // false for unreachable
                } else {
                    this.allPaths[this.entity] = new AStarPath(0, false);
                }

                // Unity says to Dispose() Temp collections:
                // https://forum.unity.com/threads/allocator-temp-container-need-dispose.852082/
                this.allNodes.Dispose();
                heapList.Dispose();
                openSetMap.Dispose();
                this.closeSet.Dispose();
            }

            private AStarNode CreateNode(int2 position, int parent, float g, float h) {
                int nodeIndex = this.allNodes.Length;
                AStarNode node = new AStarNode(nodeIndex, position, parent, g, h);
                this.allNodes.Add(node);

                return node;
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

                    if (!this.reachability.IsReachable(this.index, current.position, neighborPosition)) {
                        // Not reachable based from specified reachability
                        continue;
                    }

                    float tentativeG = current.G + this.reachability.GetWeight(this.index, current.position, neighborPosition);

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

            // Returns the position count in the path
            private int ConstructPath(AStarNode destination) {
                // Note here that we no longer need to reverse the ordering of the path
                // We just add them as reversed in AStarPath
                // AStarPath then knows how to handle this
                DynamicBuffer<Int2BufferElement> pathList = this.allPathLists[this.entity];
                pathList.Clear();
                AStarNode current = this.allNodes[destination.index];
                while (current.parent >= 0) {
                    pathList.Add(new Int2BufferElement(current.position));
                    current = this.allNodes[current.parent];
                }

                return pathList.Length;
            }

            public bool IsInCloseSet(int2 position) {
                return this.closeSet.TryGetValue(position, out _);
            }
        }

    }
}