namespace Game {
    using Common;

    using UnityEngine;

    /// <summary>
    /// Contains utility methods for a conditioned color text
    /// </summary>
    public static class TextColorDisplayUtils {

        private static readonly Color32 FAIL_COLOR = new Color32(234, 86, 22, 255);
        private static readonly Color32 PASS_COLOR = new Color32(138, 178, 0, 255);
        private static readonly Color32 EQUAL_COLOR = FAIL_COLOR;

        /// <summary>
        /// Returns the corresponding color
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static Color ResolveTextColor(int value1, int value2) {
            return value1 == value2 ? EQUAL_COLOR : (value1 > value2 ? PASS_COLOR : FAIL_COLOR);
        }

    }
}
