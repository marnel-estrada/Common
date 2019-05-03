using UnityEngine;

namespace Common {
    /**
     * Contains vector related utility methods.
     */
    public static class VectorUtils {
        // a constant vector where the components are all 0
        public static readonly Vector3 ZERO = new Vector3(0, 0, 0);

        // a constant vector where the components are all 1
        public static readonly Vector3 ONE = new Vector3(1, 1, 1);

        // the global constant UP vector
        public static readonly Vector3 UP = new Vector3(0, 1, 0);

        // the global constant RIGHT vector
        public static readonly Vector3 RIGHT = new Vector3(1, 0, 0);

        // the global constant FORWARD vector
        public static readonly Vector3 FORWARD = new Vector3(0, 0, 1);

        public static readonly Vector2 ZERO_2D = new Vector2(0, 0);

        /**
         * Returns whether or not the specified vectors a and b are equal.
         */
        public static bool Equals(Vector3 a, Vector3 b) {
            return Comparison.TolerantEquals(a.x, b.x) && Comparison.TolerantEquals(a.y, b.y) &&
                Comparison.TolerantEquals(a.z, b.z);
        }

        /// <summary>
        /// Vector2 equals comparison
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Equals(Vector2 a, Vector2 b) {
            return Comparison.TolerantEquals(a.x, b.x) && Comparison.TolerantEquals(a.y, b.y);
        }

        /// <summary>
        /// Returns a random vector that is between the specified extents
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 RandomRange(Vector2 a, Vector2 b) {
            float x = UnityEngine.Random.Range(a.x, b.x);
            float y = UnityEngine.Random.Range(a.y, b.y);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Adds a Vector2 to the Vector3
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 Add(Vector3 a, Vector2 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z);
        }
    }
}