using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that collects SpriteManager instances in a Dictionary so that it can be injected in
    /// other systems.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SpriteManagerInstancesSystem : CollectSharedComponentsSystem<SpriteManager> {
        protected override EntityQuery ResolveQuery() {
            // We added sprite as subtractive here because we don't want to count those sprites
            // where the SpriteManager is just added
            return new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Collected>()
                .WithAll<Sprite>()
                .WithNone<SpriteManager>().Build(this);
        }
    }
}
