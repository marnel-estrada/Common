using Unity.Entities;

namespace GoapBrainEcs {
    public struct AtomActionOnFail : IComponentData {
        public readonly Entity agentEntity; // We added it here for convenience on writing action systems
        public readonly Entity parentOnFailAtomActionExecution;
        
        public bool done;

        public AtomActionOnFail(Entity agentEntity, Entity parentOnFailAtomActionExecution) {
            this.agentEntity = agentEntity;
            this.parentOnFailAtomActionExecution = parentOnFailAtomActionExecution;
            this.done = false;
        }
    }
}