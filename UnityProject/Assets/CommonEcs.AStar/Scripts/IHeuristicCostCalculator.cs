namespace CommonEcs {
    public interface IHeuristicCostCalculator<in T> where T : unmanaged {
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
        float ComputeCost(T start, T goal);
    }
}