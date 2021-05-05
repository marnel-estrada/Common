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
    }
}