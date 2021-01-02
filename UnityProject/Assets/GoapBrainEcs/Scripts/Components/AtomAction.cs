using Unity.Entities;

namespace GoapBrainEcs {
    public struct AtomAction : IComponentData {
        public readonly Entity agentEntity; // We added it here for convenience on writing action systems
        public readonly Entity parentAtomActionSetExecution;

        public bool started;
        public GoapStatus status;

        public AtomAction(Entity agentEntity, Entity parentAtomActionSetExecution) {
            this.agentEntity = agentEntity;
            this.parentAtomActionSetExecution = parentAtomActionSetExecution;

            this.started = false;
            this.status = GoapStatus.RUNNING;
        }
    }
}