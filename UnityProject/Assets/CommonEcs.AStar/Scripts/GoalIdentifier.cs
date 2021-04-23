using System;

using Unity.Mathematics;

namespace CommonEcs {
    public interface GoalIdentifier<T> where T : unmanaged, IEquatable<T> {
        /// <summary>
        /// Returns whether or not the specified position is the goal
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool IsGoal(T position);
    }
}