using Unity.Entities;
using Unity.Mathematics;

namespace GoapBrainEcs {
    public class MoveIntTranslationComposer : AtomActionComposer {
        private readonly int3 target;

        public MoveIntTranslationComposer(int3 target) {
            this.target = target;
        }

        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(atomActionEntity, new MoveIntTranslation(this.target));
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