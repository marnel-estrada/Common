using Unity.Mathematics;
using UnityEngine;

namespace Common {
	/**
	 * Class for comparing floating point values
	 */
	public static class Comparison {
		/**
		 * Returns whether or not a == b
		 */
		public static bool TolerantEquals(float a, float b) {
			return math.abs(a - b) < 0.0001f;
		}

		/**
		 * Returns whether or not a >= b
		 */
		public static bool TolerantGreaterThanOrEquals(float a, float b) {
			return a > b || TolerantEquals(a, b);
		}

		/**
		 * Returns whether or not a <= b
		 */
		public static bool TolerantLesserThanOrEquals(float a, float b) {
			return a < b || TolerantEquals(a, b);
		}

		/// <summary>
		/// Returns whether or not the specified floating value is zero
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsZero(float value) {
			return TolerantEquals(value, 0.0f);
		}
	}
}
