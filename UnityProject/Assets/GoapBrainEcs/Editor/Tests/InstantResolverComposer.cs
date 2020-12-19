using Unity.Entities;

namespace GoapBrainEcs {
    public class InstantResolverComposer : IConditionResolverComposer {
        private readonly bool resolveValue;

        public InstantResolverComposer(bool resolveValue) {
            this.resolveValue = resolveValue;
        }
        
        public void Prepare(Entity resolverEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(resolverEntity, new InstantResolver(this.resolveValue));
        }
    }
}