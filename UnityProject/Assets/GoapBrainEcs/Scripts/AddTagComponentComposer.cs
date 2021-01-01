using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A generic AtomActionComposer that adds a tag component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AddTagComponentComposer<T> : AtomActionComposer where T : struct, IComponentData {
        public override void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent<T>(atomActionEntity);
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