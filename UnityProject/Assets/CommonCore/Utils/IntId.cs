using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common {
    /// <summary>
    /// A utility struct that wraps integers so it can be used as ID
    /// It can also be used as delegate for other int ID types (like a typedef but for a specific type)
    /// </summary>
    public struct IntId : IEquatable<IntId> {

        public readonly int value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value"></param>
        public IntId(int value) {
            this.value = value;
        }

        /// <summary>
        /// Returns whether or not the ID is equal to another ID
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IntId other) {
            return this.value == other.value;
        }

        public override bool Equals(object obj) {
            // Check for null values and compare run-time types
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return Equals((IntId) obj);
        }

        public override int GetHashCode() {
            return this.value;
        }

        public override string ToString() {
            return this.value.ToString();
        }

        public static bool operator ==(IntId a, IntId b) {
            return a.value == b.value;
        }

        public static bool operator !=(IntId a, IntId b) {
            return a.value != b.value;
        }

    }
}
