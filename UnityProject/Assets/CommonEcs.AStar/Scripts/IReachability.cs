using System;

namespace CommonEcs {
    /**
	 * An interface that is used by AStar to determine if a specific node is reachable. This is different from link specific reachability.
	 * This may be implemented by client code for game specific purposes which may not be determined from link information.
	 * For example, the reachability depends on that state of a specific game object.
     *
     * The index is used to locate to a NativeArray of entities or components that the Reachability
     * algorithm may need
	 */
    public interface IReachability<TNode> where TNode : unmanaged, IEquatable<TNode> {
        /// <summary>
        /// A reachability check on a single position
        /// This is used to check if a goal is reachable at all
        /// If not, the search ends abruptly
        /// This is to avoid useless search when the position can't be reached at all
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool IsReachable(TNode position);

        /// <summary>
        /// Returns whether or not the specified movement is reachable.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        bool IsReachable(TNode start, TNode destination);

        /// <summary>
        /// Computes the weight for the specified movement
        /// </summary>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        float GetWeight(TNode start, TNode destination);
    }
}