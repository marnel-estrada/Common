using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that collects SpriteManager instances in a Dictionary so that it can be injected in
    /// other systems.
    /// </summary>
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class SpriteManagerInstancesSystem : CollectSharedComponentsSystem<SpriteManager> {
        protected override EntityQuery ResolveQuery() {
            // We added sprite as subtractive here because we don't want to count those sprites
            // where the SpriteManager is just added. We just want those entities with just the
            // SpriteManager shared component.
            return new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SpriteManager>()
                .WithNone<Collected>()
                .WithNone<Sprite>().Build(this);
        }
    }
}
