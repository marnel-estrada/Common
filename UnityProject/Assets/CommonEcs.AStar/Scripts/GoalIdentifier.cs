using Unity.Mathematics;

namespace CommonEcs {
    public interface GoalIdentifier {
        /// <summary>
        /// Returns whether or not the specified position is the goal
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool IsGoal(int2 position);
    }
}