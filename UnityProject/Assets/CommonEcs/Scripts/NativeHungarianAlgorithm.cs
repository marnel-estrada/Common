using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// Hungarian algorithm but uses native collections.
    /// </summary>
    [BurstCompile]
    public static class NativeHungarianAlgorithm {
        [BurstCompile]
        public static void FindAssignments(ref NativeArray2d<float> costs, ref NativeList<int> results) {
            results.Clear();
            
            int height = costs.rows;
            int width = costs.columns;
            bool rowsGreaterThanCols = height > width;
            if (rowsGreaterThanCols) {
                // make sure cost matrix has number of rows greater than columns
                int row = width;
                int col = height;
                NativeArray2d<float> transposeCosts = new(col, row, Allocator.Temp);
                for (int y = 0; y < row; y++) {
                    for (int x = 0; x < col; x++) {
                        transposeCosts[x, y] = costs[y, x];
                    }
                }

                height = row;
                width = col;

                FindAssignmentsInternal(ref transposeCosts, ref results, height, width, true);
            } else {
                FindAssignmentsInternal(ref costs, ref results, height, width, false);
            }
        }

        [BurstCompile]
        private static void FindAssignmentsInternal(ref NativeArray2d<float> costs, ref NativeList<int> results, 
            int height, int width, bool rowsGreaterThanCols) {
            for (int y = 0; y < height; y++) {
                float min = float.MaxValue;

                for (int x = 0; x < width; x++) {
                    min = math.min(min, costs[x, y]);
                }

                for (int x = 0; x < width; x++) {
                    costs[x, y] = costs[x, y] - min;
                }
            }

            NativeArray2d<byte> masks = new(width, height, Allocator.Temp);
            NativeArray<bool> rowsCovered = new(height, Allocator.Temp);
            NativeArray<bool> colsCovered = new(width, Allocator.Temp);

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (costs[x, y].IsZero() && !rowsCovered[y] && !colsCovered[x]) {
                        masks[x, y] = 1;
                        rowsCovered[y] = true;
                        colsCovered[x] = true;
                    }
                }
            }

            ClearCovers(ref rowsCovered, ref colsCovered, width, height);

            NativeArray<Location> paths = new(width * height, Allocator.Temp);
            Location pathStart = default;
            int step = 1;

            int counter = 0;
            //const int limit = 100000;
            while (step != -1 /*&& counter < limit*/) {
                step = step switch {
                    1 => RunStep1(ref masks, ref colsCovered, width, height),
                    2 => RunStep2(ref costs, ref masks, ref rowsCovered, ref colsCovered, width, height, ref pathStart),
                    3 => RunStep3(ref masks, ref rowsCovered, ref colsCovered, width, height, ref paths, pathStart),
                    4 => RunStep4(ref costs, ref rowsCovered, ref colsCovered, width, height),
                    _ => step
                };
                ++counter;
            }

            // if (counter >= limit) {
            //     // Reached the limit
            //     throw new Exception("Something is wrong");
            // }

            NativeArray<int> agentTasks = new(height, Allocator.Temp);
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (masks[x, y] == 1) {
                        agentTasks[y] = x;
                        break;
                    } 
                    
                    agentTasks[y] = -1;
                }
            }

            if (rowsGreaterThanCols) {
                NativeArray<int> agentTasksTranspose = new(width, Allocator.Temp);
                for (int x = 0; x < width; x++) {
                    agentTasksTranspose[x] = -1;
                }

                for (int y = 0; y < height; y++) {
                    agentTasksTranspose[agentTasks[y]] = y;
                }

                results.AddRange(agentTasksTranspose);
            } else {
                results.AddRange(agentTasks);
            }
        }

        [BurstCompile]
        private static int RunStep1(ref NativeArray2d<byte> masks, ref NativeArray<bool> colsCovered, int width, int height) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (masks[x, y] == 1)
                        colsCovered[x] = true;
                }
            }

            int colsCoveredCount = 0;

            for (int x = 0; x < width; x++) {
                if (colsCovered[x])
                    colsCoveredCount++;
            }

            if (colsCoveredCount == math.min(width, height))
                return -1;

            return 2;
        }
        
        [BurstCompile]
        private static int RunStep2(ref NativeArray2d<float> costs, ref NativeArray2d<byte> masks, 
            ref NativeArray<bool> rowsCovered, ref NativeArray<bool> colsCovered, int w, int h,
            ref Location pathStart) {
            while (true) {
                FindZero(ref costs, ref rowsCovered, ref colsCovered, w, h, out Location loc);
                if (loc.row == -1)
                    return 4;

                masks[loc.column, loc.row] = 2;

                int starCol = FindStarInRow(ref masks, w, loc.row);
                if (starCol != -1) {
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                } else {
                    pathStart = loc;
                    return 3;
                }
            }
        }
        
        [BurstCompile]
        private static int RunStep3(ref NativeArray2d<byte> masks, ref NativeArray<bool> rowsCovered, 
            ref NativeArray<bool> colsCovered, int w, int h, ref NativeArray<Location> paths, in Location pathStart) {
            int pathIndex = 0;
            paths[0] = pathStart;

            while (true) {
                int row = FindStarInColumn(ref masks, h, paths[pathIndex].column);
                if (row == -1)
                    break;

                pathIndex++;
                paths[pathIndex] = new Location(row, paths[pathIndex - 1].column);

                int col = FindPrimeInRow(ref masks, w, paths[pathIndex].row);

                pathIndex++;
                paths[pathIndex] = new Location(paths[pathIndex - 1].row, col);
            }

            ConvertPath(ref masks, ref paths, pathIndex + 1);
            ClearCovers(ref rowsCovered, ref colsCovered, w, h);
            ClearPrimes(ref masks, w, h);

            return 1;
        }
        
        [BurstCompile]
        private static int RunStep4(ref NativeArray2d<float> costs, ref NativeArray<bool> rowsCovered, 
            ref NativeArray<bool> colsCovered, int width, int height) {
            float minValue = FindMinimum(ref costs, ref rowsCovered, ref colsCovered, width, height);

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (rowsCovered[y])
                        costs[x, y] = costs[x, y] + minValue;
                    if (!colsCovered[x])
                        costs[x, y] = costs[x, y] - minValue;
                }
            }

            return 2;
        }
        
        [BurstCompile]
        private static float FindMinimum(ref NativeArray2d<float> costs, ref NativeArray<bool> rowsCovered, 
            ref NativeArray<bool> colsCovered, int width, int height) {
            float minValue = float.MaxValue;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (!rowsCovered[y] && !colsCovered[x])
                        minValue = math.min(minValue, costs[x, y]);
                }
            }

            return minValue;
        }
        
        [BurstCompile]
        private static int FindStarInRow(ref NativeArray2d<byte> masks, int width, int row) {
            for (int x = 0; x < width; x++) {
                if (masks[x, row] == 1)
                    return x;
            }

            return -1;
        }
        
        [BurstCompile]
        private static int FindStarInColumn(ref NativeArray2d<byte> masks, int height, int col) {
            for (int y = 0; y < height; y++) {
                if (masks[col, y] == 1)
                    return y;
            }

            return -1;
        }
        
        [BurstCompile]
        private static int FindPrimeInRow(ref NativeArray2d<byte> masks, int width, int row) {
            for (int x = 0; x < width; x++) {
                if (masks[x, row] == 2)
                    return x;
            }

            return -1;
        }
        
        [BurstCompile]
        private static void FindZero(ref NativeArray2d<float> costs, ref NativeArray<bool> rowsCovered, 
            ref NativeArray<bool> colsCovered, int width, int height, out Location result) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (costs[x, y].TolerantEquals(0) && !rowsCovered[y] && !colsCovered[x]) {
                        result = new Location(y, x);
                        return;
                    }
                }
            }

            result = new Location(-1, -1);
        }
        
        [BurstCompile]
        private static void ConvertPath(ref NativeArray2d<byte> masks, ref NativeArray<Location> paths, int pathLength) {
            for (int i = 0; i < pathLength; i++) {
                masks[paths[i].column, paths[i].row] = masks[paths[i].column, paths[i].row] switch {
                    1 => 0,
                    2 => 1,
                    _ => masks[paths[i].column, paths[i].row]
                };
            }
        }
        
        [BurstCompile]
        private static void ClearPrimes(ref NativeArray2d<byte> masks, int width, int height) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (masks[x, y] == 2)
                        masks[x, y] = 0;
                }
            }
        }
        
        [BurstCompile]
        private static void ClearCovers(ref NativeArray<bool> rowsCovered, ref NativeArray<bool> colsCovered, int w, int h) {
            for (int y = 0; y < h; y++) {
                rowsCovered[y] = false;
            }

            for (int x = 0; x < w; x++) {
                colsCovered[x] = false;
            }
        }
        
        private readonly struct Location {
            internal readonly int row;
            internal readonly int column;

            internal Location(int row, int col) {
                this.row = row;
                this.column = col;
            }
        }
    }
}