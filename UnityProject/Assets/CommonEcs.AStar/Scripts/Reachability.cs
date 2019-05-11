using Unity.Mathematics;

namespace CommonEcs {
    /**
	 * An interface that is used by AStar to determine if a specific node is reachable. This is different from link specific reachability.
	 * This may be implemented by client code for game specific purposes which may not be determined from link information.
	 * For example, the reachability depends on that state of a specific game object.
	 */
    public interface Reachability {
        /// <summary>
        /// A reachability check on a single position
        /// This is used to check if a goal is reachable at all
        /// If not, the search ends abruptly
        /// This is to avoid useless search when the position can't be reached at all
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        bool IsReachable(int2 goal);

        /// <summary>
        /// Returns whether or not the specified movement is reachable.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        bool IsReachable(int2 start, int2 destination);

        /// <summary>
        /// Computes the weight for the specified movement
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        float GetWeight(int2 from, int2 to);
    }
}