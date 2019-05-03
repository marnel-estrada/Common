using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Provides methods to generate Gaussian random numbers
    /// Algorithm taken from here http://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
    /// </summary>
    public static class Gaussian {

        /// <summary>
        /// Generates a random number under Gaussian distribution
        /// </summary>
        /// <returns></returns>
        public static float NextValue() {
            float v1, v2, s;
            do {
                v1 = 2.0f * UnityEngine.Random.Range(0f, 1f) - 1.0f;
                v2 = 2.0f * UnityEngine.Random.Range(0f, 1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0f || s == 0f);

            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
 
            return v1 * s;
        }

        /// <summary>
        /// Generates a Gaussian random number with specified mean and standard deviation
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="standardDeviation"></param>
        /// <returns></returns>
        public static float NextValue(float mean, float standardDeviation) {
            return mean + NextValue() * standardDeviation;
        }

    }
}
