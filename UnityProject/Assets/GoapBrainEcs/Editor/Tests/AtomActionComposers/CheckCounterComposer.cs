using Unity.Entities;

namespace GoapBrainEcs {
    public class CheckCounterComposer : AtomActionComposer {
        private readonly int valueToCheck;

        public CheckCounterComposer(int valueToCheck) {
            this.valueToCheck = valueToCheck;
        }
        
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new CheckCounter(this.valueToCheck));
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