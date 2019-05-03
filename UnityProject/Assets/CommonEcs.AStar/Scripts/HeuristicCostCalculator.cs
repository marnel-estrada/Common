using Unity.Mathematics;

namespace CommonEcs {
    public interface HeuristicCostCalculator {
        /// <summary>
        /// Computes the heuristic cost from the specified starting position and the goal.
        /// </summary>
        /// <returns>
        /// The cost.
        /// </returns>
        /// <param name='start'>
        /// Node.
        /// </param>
        /// <param name='goal'>
        /// Goal.
        /// </param>
        float ComputeCost(int2 start, int2 goal);
    }
}