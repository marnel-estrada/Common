using Unity.Entities;

namespace GoapBrainEcs {
    public class RunningResolverComposer : IConditionResolverComposer {
        public readonly bool resolutionValue;

        public RunningResolverComposer(bool resolutionValue) {
            this.resolutionValue = resolutionValue;
        }
        
        public void Prepare(Entity resolverEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(resolverEntity, new RunningResolver(this.resolutionValue));
        }
    }
}