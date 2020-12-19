using Unity.Entities;
using Unity.Mathematics;

namespace GoapBrainEcs {
    public struct IntTranslation : IComponentData {
        public int3 value;
    }
}