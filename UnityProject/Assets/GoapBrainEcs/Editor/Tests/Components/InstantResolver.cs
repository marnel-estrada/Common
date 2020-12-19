using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    public struct InstantResolver : IComponentData {
        public readonly ByteBool resolveValue;

        public InstantResolver(bool resolveValue) {
            this.resolveValue = resolveValue;
        }
    }
}