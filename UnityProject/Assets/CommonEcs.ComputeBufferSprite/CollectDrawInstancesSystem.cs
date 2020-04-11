using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(AddComputeBufferSpriteToDrawInstanceSystem))]
    public class CollectDrawInstancesSystem : CollectSharedComponentsSystem<ComputeBufferDrawInstance> {
        protected override EntityQuery ResolveQuery() {
            return GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Collected), typeof(ComputeBufferSprite)
            }, new ComponentType[] {
                typeof(ComputeBufferDrawInstance)
            }));
        }
    }
}