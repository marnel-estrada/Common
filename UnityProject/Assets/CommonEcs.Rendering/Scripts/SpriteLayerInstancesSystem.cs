using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteLayerInstancesSystem : CollectSharedComponentsSystem<SpriteLayer> {
    }
}