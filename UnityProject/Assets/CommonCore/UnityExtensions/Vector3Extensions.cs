using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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

    }
}
