using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// Common utilities for DOTS debugging
    /// </summary>
    public static class DotsDebug {
        /// <summary>
        /// Logs an int3. Uses string.Format() to avoid burst exception.
        /// </summary>
        /// <param name="p"></param>
        public static void Log(in int3 p) {
            // ReSharper disable once UseStringInterpolation (due to Burst)
            Debug.Log(string.Format("({0}, {1}, {2})", p.x, p.y, p.z));
        }
    }
}