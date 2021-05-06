﻿using UnityEngine;

namespace Common {
    /// <summary>
    /// Contains Vector3 extension methods
    /// </summary>
    public static class Vector3Extensions {
        /// <summary>
        /// Computes for the the 2D magnitude of the specified Vector3 (ignores z)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Magnitude2D(this Vector3 v) {
            return Mathf.Sqrt((v.x * v.x) + (v.y * v.y));
        }

        /**
         * Returns whether or not the specified vectors a and b are equal.
         */
        public static bool TolerantEquals(this Vector3 a, Vector3 b) {
            return a.x.TolerantEquals(b.x) && a.y.TolerantEquals(b.y) &&
                   a.z.TolerantEquals(b.z);
        }

        /// <summary>
        /// Adds a Vector2 to the Vector3
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 Add(this Vector3 a, Vector2 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z);
        }
    }
}
