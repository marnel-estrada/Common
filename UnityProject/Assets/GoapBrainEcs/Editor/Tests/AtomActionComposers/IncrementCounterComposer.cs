using Unity.Entities;

namespace GoapBrainEcs {
    public class IncrementCounterComposer : AtomActionComposer {
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new IncrementCounter());
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