using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    public struct RunningResolver : IComponentData {
        public byte counter;
        public ByteBool resolutionValue;

        public RunningResolver(bool resolutionValue) {
            this.counter = 0;
            this.resolutionValue = resolutionValue;
        }
    }
}