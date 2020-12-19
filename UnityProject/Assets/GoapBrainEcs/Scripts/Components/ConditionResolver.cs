using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    public struct ConditionResolver : IComponentData {
        // The entity with the GoapAgent that request the action search (thus this condition resolution)
        public readonly Entity agentEntity;
        
        // This is the action search that requested the condition resolution
        public readonly Entity actionSearchEntity;
        
        public ConditionResolverStatus status;
        public ByteBool result;

        public ConditionResolver(Entity agentEntity, Entity actionSearchEntity) {
            this.agentEntity = agentEntity;
            this.actionSearchEntity = actionSearchEntity;
            
            this.status = ConditionResolverStatus.RUNNING;
            this.result = false;
        }
    }
}