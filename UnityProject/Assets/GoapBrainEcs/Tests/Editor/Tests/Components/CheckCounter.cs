using Unity.Entities;

namespace GoapBrainEcs {
    public readonly struct CheckCounter : IComponentData {
        public readonly int valueToCheck;

        public CheckCounter(int valueToCheck) {
            this.valueToCheck = valueToCheck;
        }
    }
}