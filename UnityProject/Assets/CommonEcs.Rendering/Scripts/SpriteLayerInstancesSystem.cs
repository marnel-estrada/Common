using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteLayerInstancesSystem : CollectSharedComponentsSystem<SpriteLayer> {
    }
}