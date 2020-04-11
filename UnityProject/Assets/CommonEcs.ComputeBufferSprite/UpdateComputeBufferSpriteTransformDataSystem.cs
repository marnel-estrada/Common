using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateAfter(typeof(TRSToLocalToWorldSystem))]
    [UpdateAfter(typeof(TRSToLocalToParentSystem))]
    public class UpdateComputeBufferSpriteTransformDataSystem : SystemBase {
        protected override void OnUpdate() {
            // Transform only non static sprites
            this.Entities.WithNone<Static>().ForEach(delegate(ref ComputeBufferSprite sprite, ref LocalToWorld transform) {
                sprite.localToWorld = transform.Value;
            }).WithBurst().ScheduleParallel(); 
        }
    }
}