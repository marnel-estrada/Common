using UnityEngine;

namespace Common {
    public class Bezier {
        /// <summary>
        /// Quadratic bezier curve
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Quadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            // This is from https://en.wikipedia.org/wiki/B%C3%A9zier_curve under Quadratic Bezier Curves
            float oneMinusT = 1.0f - t;
            return ((oneMinusT * oneMinusT) * p0) + ((2 * oneMinusT * t) * p1) + (t * t * p2);
        }

        /// <summary>
        /// Cubic bezier curve
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Cubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            // This is from https://en.wikipedia.org/wiki/B%C3%A9zier_curve under cubic Bezier Curves
            float oneMinusT = 1.0f - t;
            return oneMinusT * Quadratic(p0, p1, p2, t) + t * Quadratic(p1, p2, p3, t);
        }
    }
}