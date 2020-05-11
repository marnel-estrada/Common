using System;

using Unity.Mathematics;

using UnityEngine;

namespace Common.Math {
	[Serializable]
	public class IntVector2 : IEquatable<IntVector2> {
		public static readonly IntVector2 ZERO = new IntVector2(0, 0);
		
		[SerializeField]
		public int x;
		
		[SerializeField]
		public int y;
		
		/**
		 * Default constructor
		 */
		public IntVector2() : this(0, 0) {
		}
		
		/**
		 * Constructor with specified coordinates.
		 */
		public IntVector2(int x, int y) {
			this.x = x;
			this.y = y;
		}
		
		/**
		 * Copy constructor
		 */
		public IntVector2(IntVector2 other) : this(other.x, other.y) {
		}
		
		/**
		 * Sets the values of this vector using the specified one.
		 */
		public void Set(IntVector2 other) {
			if (other == null) {
				return;
			}
			
			this.x = other.x;
			this.y = other.y;
		}

        /// <summary>
        /// Sets the coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Set(int x, int y) {
            this.x = x;
            this.y = y;
        }
		
		/**
		 * Returns whether or not this IntVector2 is equal to the specified one.
		 */
		public bool Equals(IntVector2 other) {
			if(other == null) {
				return false;
			}
			
			return this.x == other.x && this.y == other.y;
		}

        public override bool Equals(object obj) {
            // Check for null values and compare run-time types
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return Equals((IntVector2)obj);
        }

        public override int GetHashCode() {
            return Hash(this.x, this.y);
        }

		/// <summary>
        /// Common algorithm for hashing two integers
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int Hash(int x, int y) {
            // This is using Szudzik's function found in http://stackoverflow.com/questions/919612/mapping-two-integers-to-one-in-a-unique-and-deterministic-way
            uint A = (uint)(x >= 0 ? 2 * x : -2 * x - 1);
            uint B = (uint)(y >= 0 ? 2 * y : -2 * y - 1);
            int C = (int)((A >= B ? A * A + A + B : A + B * B) / 2);
            return x < 0 && y < 0 || x >= 0 && y >= 0 ? C : -C - 1;
        }

        public override string ToString() {
			return "IntVector2 (" + x + ", " + y + ")";
		}

        public float SquareDistanceTo(IntVector2 other) {
            float xDiff = other.x - this.x;
            float yDiff = other.y - this.y;

            return (xDiff * xDiff) + (yDiff * yDiff);
        }

        public float DistanceTo(IntVector2 other) {
            return Mathf.Sqrt(this.SquareDistanceTo(other));
        }

		public string CoordinateToString() {
			return "(" + x + ", " + y + ")";
		}

		public int2 AsInt2() {
			return new int2(this.x, this.y);
		}
    }
}
