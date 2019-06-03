using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that collects SpriteManager instances in a Dictionary so that it can be injected in
    /// other systems.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteManagerInstancesSystem : CollectSharedComponentsSystem<SpriteManager> {
        protected override EntityQuery ResolveQuery() {
            // We added sprite as subtractive here because we don't want to count those sprites
            // where the SpriteManager is just added
            return GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Collected),
                typeof(Sprite)
            }, new ComponentType[] {
                typeof(SpriteManager)
            }));
        }
    }
}
