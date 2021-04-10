namespace GoapBrain {
    struct ConditionChange {
        public readonly ConditionId id;
        public readonly bool previousValue;
        public readonly bool updatedValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="previousValue"></param>
        /// <param name="updatedValue"></param>
        public ConditionChange(ConditionId id, bool previousValue, bool updatedValue) {
            this.id = id;
            this.previousValue = previousValue;
            this.updatedValue = updatedValue;
        }
    }
}
