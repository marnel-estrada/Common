using Unity.Entities;
using Unity.Mathematics;

namespace GoapBrainEcs {
    public readonly struct MoveIntTranslation : IComponentData {
        public readonly int3 target;

        public MoveIntTranslation(int3 target) {
            this.target = target;
        }
    }
}