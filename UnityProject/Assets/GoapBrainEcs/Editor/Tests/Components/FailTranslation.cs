using Unity.Entities;

namespace GoapBrainEcs {
    public readonly struct FailTranslation : IComponentData {
        public readonly Entity agentEntity;

        public FailTranslation(Entity agentEntity) {
            this.agentEntity = agentEntity;
        }
    }
}