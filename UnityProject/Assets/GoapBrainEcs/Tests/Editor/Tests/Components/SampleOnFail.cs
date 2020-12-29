using Unity.Entities;

namespace GoapBrainEcs {
    public struct SampleOnFail : IComponentData {
        // The entity with the Counter component
        public readonly Entity counterEntity;

        public SampleOnFail(Entity counterEntity) {
            this.counterEntity = counterEntity;
        }
    }
}