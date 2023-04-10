using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class SpriteLayerInstancesSystem : CollectSharedComponentsSystem<SpriteLayer> {
    }
}