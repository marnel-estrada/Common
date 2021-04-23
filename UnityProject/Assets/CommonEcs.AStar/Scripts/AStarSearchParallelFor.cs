using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [BurstCompile]
    public struct AStarSearchParallelFor<THeuristicCalculator, TReachability> : IJobParallelFor
        where THeuristicCalculator : struct, IHeuristicCostCalculator<int3> 
        where TReachability : struct, IReachability<int3> {
        [ReadOnly]
        public NativeArray<Entity> entities; // The request entities

        [NativeDisableParallelForRestriction, ReadOnly]
        public ComponentDataFromEntity<AStarSearchParameters> allParameters;

        [NativeDisableParallelForRestriction, WriteOnly]
        public ComponentDataFromEntity<AStarPath> allPaths;

        [NativeDisableParallelForRestriction, WriteOnly]
        public ComponentDataFromEntity<Waiting> allWaiting;

        [NativeDisableParallelForRestriction, WriteOnly]
        public BufferFromEntity<Int3BufferElement> allPathLists;

        [ReadOnly]
        public TReachability reachability;

        private THeuristicCalculator heuristicCalculator;

        // This will be specified by client on whether it wants to include diagonal neighbors
        [ReadOnly]
        public NativeArray<int3> neighborOffsets;

        [ReadOnly]
        public MultipleGrid2dWrapper gridWrapper;

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
            public BufferFromEntity<Int3BufferElement> allPathLists;

            public TReachability reachability;
            public THeuristicCalculator heuristicCalculator;
            public NativeArray<int3> neighborOffsets;

            public MultipleGrid2dWrapper gridWrapper;

            // This is the master container for all AStarNodes. The key is the hash code of the position.
            // This will be specified by client code
            private NativeList<AStarNode<int3>> allNodes;

            private OpenSet<int3> openSet;

            // Only used for existence of position in closed set
            // This will be specified by client code
            private NativeHashMap<int3, byte> closeSet;

            private int3 goalPosition;

            public void Execute() {
                // Instantiate containers
                this.allNodes = new NativeList<AStarNode<int3>>(10, Allocator.Temp);
                
                NativeList<HeapNode<int3>> heapList = new NativeList<HeapNode<int3>>(10, Allocator.Temp);
                GrowingHeap<int3> heap = new GrowingHeap<int3>(heapList);

                NativeHashMap<int3, AStarNode<int3>> openSetMap = new NativeHashMap<int3, AStarNode<int3>>(10, Allocator.Temp);
                this.openSet = new OpenSet<int3>(heap, openSetMap);
                
                this.closeSet = new NativeHashMap<int3, byte>(10, Allocator.Temp);

                AStarSearchParameters parameters = this.allParameters[this.entity];
                this.goalPosition = parameters.goal;

                float startNodeH = this.heuristicCalculator.ComputeCost(parameters.start, this.goalPosition);
                AStarNode<int3> startNode = CreateNode(parameters.start, -1, 0, startNodeH);
                this.openSet.Push(startNode);

                float minH = float.MaxValue;
                Maybe<AStarNode<int3>> minHPosition = Maybe<AStarNode<int3>>.Nothing;

                // Process while there are nodes in the open set
                while (this.openSet.HasItems) {
                    AStarNode<int3> current = this.openSet.Pop();
                    if (current.position.Equals(this.goalPosition)) {
                        // Goal has been found
                        this.allPaths[this.entity] = new AStarPath(true);

                        return;
                    }

                    ProcessNode(current);

                    this.closeSet.TryAdd(current.position, 0);

                    // We save the node with the least H so we could still try to locate
                    // the nearest position to the destination 
                    if (current.H < minH) {
                        minHPosition = new Maybe<AStarNode<int3>>(current);
                        minH = current.H;
                    }
                }

                // Open set has been exhausted. Path is unreachable.
                if (minHPosition.HasValue) {
                    ConstructPath(minHPosition.Value);
                    this.allPaths[this.entity] = new AStarPath(false); // false for unreachable
                } else {
                    this.allPaths[this.entity] = new AStarPath(false);
                }

                // Unity says to Dispose() Temp collections:
                // https://forum.unity.com/threads/allocator-temp-container-need-dispose.852082/
                this.allNodes.Dispose();
                heapList.Dispose();
                openSetMap.Dispose();
                this.closeSet.Dispose();
            }

            private AStarNode<int3> CreateNode(int3 position, int parent, float g, float h) {
                int nodeIndex = this.allNodes.Length;
                AStarNode<int3> node = new AStarNode<int3>(nodeIndex, position, parent, g, h);
                this.allNodes.Add(node);

                return node;
            }

            private void ProcessNode(in AStarNode<int3> current) {
                if (IsInCloseSet(current.position)) {
                    // Already in closed set. We no longer process because the same node with lower F
                    // might have already been processed before. Note that we don't fix the heap. We just
                    // keep on pushing nodes with lower scores.
                    return;
                }

                // Process neighbors
                for (int i = 0; i < this.neighborOffsets.Length; ++i) {
                    int3 neighborPosition = current.position + this.neighborOffsets[i];

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

                    float h = this.heuristicCalculator.ComputeCost(neighborPosition, this.goalPosition);

                    if (this.openSet.TryGet(neighborPosition, out AStarNode<int3> existingNode)) {
                        // This means that the node is already in the open set
                        // We update the node if the current movement is better than the one in the open set
                        if (tentativeG < existingNode.G) {
                            // Found a better path. Replace the values.
                            // Note that creation automatically replaces the node at that position
                            AStarNode<int3> betterNode = CreateNode(neighborPosition, current.index, tentativeG, h);

                            // Only add to open set if it's a better movement
                            // If we just push without checking, a node with the same g score will be pushed
                            // which causes infinite loop as every node will be pushed
                            this.openSet.Push(betterNode);
                        }
                    } else {
                        AStarNode<int3> neighborNode = CreateNode(neighborPosition, current.index, tentativeG, h);
                        this.openSet.Push(neighborNode);
                    }
                }
            }

            // Returns the position count in the path
            private int ConstructPath(AStarNode<int3> destination) {
                // Note here that we no longer need to reverse the ordering of the path
                // We just add them as reversed in AStarPath
                // AStarPath then knows how to handle this
                DynamicBuffer<Int3BufferElement> pathList = this.allPathLists[this.entity];
                pathList.Clear();
                AStarNode<int3> current = this.allNodes[destination.index];
                while (current.parent >= 0) {
                    pathList.Add(new Int3BufferElement(current.position));
                    current = this.allNodes[current.parent];
                }

                return pathList.Length;
            }

            public bool IsInCloseSet(int3 position) {
                return this.closeSet.TryGetValue(position, out _);
            }
        }

    }
}