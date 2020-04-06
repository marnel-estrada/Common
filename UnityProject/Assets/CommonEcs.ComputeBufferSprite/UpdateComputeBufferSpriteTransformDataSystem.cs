using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateAfter(typeof(TRSToLocalToWorldSystem))]
    [UpdateAfter(typeof(TRSToLocalToParentSystem))]
    [UpdateBefore(typeof(PopulateSpriteListSystem))]
    public class UpdateComputeBufferSpriteTransformDataSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.ForEach(delegate(ref ComputeBufferSprite sprite, ref LocalToWorld transform) {
                sprite.localToWorld = transform.Value;
            }).ScheduleParallel();
        }
    }
}