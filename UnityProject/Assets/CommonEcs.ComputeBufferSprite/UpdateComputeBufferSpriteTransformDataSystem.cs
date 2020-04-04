using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(PopulateSpriteListSystem))]
    public class UpdateComputeBufferSpriteTransformDataSystem : SystemBase {
        protected override void OnUpdate() {
            // TODO Set sprite transform and rotation here based on Translation and Rotation components
        }
    }
}