using System;

namespace CommonEcs {
    /// <summary>
    /// A utility data that maintains tags in an integer bitmask.
    /// </summary>
    [Serializable]
    public struct Tags : IEquatable<Tags> {
        // These members are set to public so that we can check them in the entity debugger
        // We need this to ensure that the TagSet used for it is the intended one
        public readonly int tagSetId;
        public BitArray32 values;

        public Tags(in TagSet tagSet) : this() {
            this.tagSetId = tagSet.id;
        }

        /// <summary>
        /// Adds a tag
        /// </summary>
        /// <param name="tagSet"></param>
        /// <param name="tagHashCode"></param>
        public void Add(in TagSet tagSet, int tagHashCode) {
            DotsAssert.IsTrue(tagSet.id == this.tagSetId); // Ensure that we don't use another TagSet
            int index = tagSet.GetIndex(tagHashCode);
            this.values[index] = true; // Maintain tag as a bit mask
        }

        public readonly bool Contains(in TagSet tagSet, int tagHashCode) {
            DotsAssert.IsTrue(tagSet.id == this.tagSetId); // Ensure that we don't use another TagSet
            int index = tagSet.GetIndex(tagHashCode);
            return this.values[index];
        }

        public readonly bool Contains(in Tags tagsToCheck) {
            // We can simply check if the tags in tagsToCheck are contained
            // by using & on the bit array and check if it's equal (meaning all bits were retained)
            return (this.values.InternalValue & tagsToCheck.values.InternalValue) == tagsToCheck.values.InternalValue;
        }

        public bool Equals(Tags other) {
            return this.tagSetId == other.tagSetId && this.values.Equals(other.values);
        }

        public override bool Equals(object? obj) {
            return obj is Tags other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.tagSetId * 397) ^ this.values.GetHashCode();
            }
        }

        public static bool operator ==(Tags left, Tags right) {
            return left.Equals(right);
        }

        public static bool operator !=(Tags left, Tags right) {
            return !left.Equals(right);
        }
    }
}