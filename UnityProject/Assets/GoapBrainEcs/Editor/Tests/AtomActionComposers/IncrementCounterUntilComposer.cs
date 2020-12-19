using Unity.Entities;

namespace GoapBrainEcs {
    public class IncrementCounterUntilComposer : AtomActionComposer {
        private readonly int maxValue;

        public IncrementCounterUntilComposer(int maxValue) {
            this.maxValue = maxValue;
        }

        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new IncrementCounterUntil(this.maxValue));
        }
        
        public override bool HasOnFailAction {
            get {
                return false;
            }
        }

        public override void PrepareOnFailAction(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
        }
    }
}