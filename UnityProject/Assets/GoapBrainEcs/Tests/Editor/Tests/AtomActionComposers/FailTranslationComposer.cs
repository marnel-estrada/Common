using Unity.Entities;

namespace GoapBrainEcs {
    public class FailTranslationComposer : AtomActionComposer {
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new FailTranslation(agentEntity));
        }

        public override bool HasOnFailAction {
            get {
                return true;
            }
        }

        public override void PrepareOnFailAction(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new FailTranslation(agentEntity));
        }
    }
}