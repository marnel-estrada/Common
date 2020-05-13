using Unity.Entities;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateAfter(typeof(TRSToLocalToWorldSystem))]
    [UpdateAfter(typeof(TRSToLocalToParentSystem))]
    public class UpdateComputeBufferSpriteTransformSystem : SystemBase {
        protected override void OnUpdate() {
            // Transform only non static sprites
            this.Entities.WithNone<Static>().ForEach(delegate(ref ComputeBufferSprite sprite, ref Translation translation, ref NonUniformScale scale) {
                sprite.SetTransform(translation.Value.xy, scale.Value.xy);
            }).WithBurst().ScheduleParallel(); 
        }
    }
}