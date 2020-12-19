using Unity.Entities;

namespace GoapBrainEcs {
    public class DummyAtomActionComposer : AtomActionComposer {
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            // Does nothing really. Just so we could actions by having this as a dummy composer.
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