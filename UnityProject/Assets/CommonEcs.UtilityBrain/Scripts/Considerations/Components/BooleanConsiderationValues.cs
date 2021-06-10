using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// This is a component that can be added to consideration entities to hold the utility values
    /// when the consideration is a boolean one.
    /// </summary>
    public readonly struct BooleanConsiderationValues : IComponentData {
        public readonly UtilityValue trueValue;
        public readonly UtilityValue falseValue;

        public BooleanConsiderationValues(UtilityValue trueValue, UtilityValue falseValue) {
            this.trueValue = trueValue;
            this.falseValue = falseValue;
        }
    }
}