namespace CommonEcs {
    /// <summary>
    /// A utility data that maintains tags in an integer bitmask.
    /// </summary>
    public struct Tags {
        // We need this to ensure that the TagSet used for it is the intended one
        private readonly int tagSetId;
        private BitArray32 values;

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

        public bool Contains(in TagSet tagSet, int tagHashCode) {
            DotsAssert.IsTrue(tagSet.id == this.tagSetId); // Ensure that we don't use another TagSet
            int index = tagSet.GetIndex(tagHashCode);
            return this.values[index];
        }

        public bool Contains(in TagSet tagSet, in Tags tagsToCheck) {
            DotsAssert.IsTrue(tagSet.id == this.tagSetId); // Ensure that we don't use another TagSet
            DotsAssert.IsTrue(tagSet.id == tagsToCheck.tagSetId); // Ensure that we don't use another TagSet
            
            // We can simply check if the tags in tagsToCheck are contained
            // by using & on the bit array and check if it's equal (meaning all bits were retained)
            return (this.values.InternalValue & tagsToCheck.values.InternalValue) == tagsToCheck.values.InternalValue;
        }
    }
}