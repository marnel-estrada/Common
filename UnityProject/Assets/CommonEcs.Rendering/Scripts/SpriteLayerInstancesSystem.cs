using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SpriteLayerInstancesSystem : CollectSharedComponentsSystem<SpriteLayer> {
    }
}