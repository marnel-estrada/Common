using Unity.Entities;

namespace GoapBrainEcs {
    public struct IncrementCounterUntil : IComponentData {
        public readonly int maxValue;

        public IncrementCounterUntil(int maxValue) {
            this.maxValue = maxValue;
        }
    }
}