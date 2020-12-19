using Unity.Entities;

namespace GoapBrainEcs {
    public class SampleOnFailComposer : AtomActionComposer {
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new SampleOnFail(agentEntity));
        }

        public override bool HasOnFailAction {
            get {
                return true;
            }
        }
        
        public override void PrepareOnFailAction(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new SampleOnFail(agentEntity));
        }
    }
}