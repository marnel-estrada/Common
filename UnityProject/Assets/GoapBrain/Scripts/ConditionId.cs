using System;

namespace GoapBrain {
    /// <summary>
    /// A custom id for all GOAP conditions
    /// </summary>
    [Serializable]
    public struct ConditionId : IEquatable<ConditionId> {
        private readonly int value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value"></param>
        public ConditionId(int value) {
            this.value = value;
        }

        public int Value {
            get {
                return this.value;
            }
        }

        /// <summary>
        /// Returns whether or not the ID is equal to another ID
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ConditionId other) {
            return this.value == other.value;
        }

        public override bool Equals(object obj) {
            // Check for null values and compare run-time types
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return Equals((ConditionId)obj);
        }

        public override int GetHashCode() {
            return this.value;
        }

        public override string ToString() {
            return this.value.ToString();
        }

        public static bool operator ==(ConditionId a, ConditionId b) {
            return a.value == b.value;
        }

        public static bool operator !=(ConditionId a, ConditionId b) {
            return a.value != b.value;
        }
    }
}
