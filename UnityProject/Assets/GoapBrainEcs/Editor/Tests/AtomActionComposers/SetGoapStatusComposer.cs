using Unity.Entities;

namespace GoapBrainEcs {
    public class SetGoapStatusComposer : AtomActionComposer {
        private readonly GoapStatus status;

        public SetGoapStatusComposer(GoapStatus status) {
            this.status = status;
        }
        
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new SetGoapStatus(this.status));
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