using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Removes the Changed component
    /// Performed after rendering
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateAfter(typeof(SpriteManagerRendererSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RemoveSpriteChangedSystem : ComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkEntityType entityType;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(Sprite), typeof(Changed));
        }

        protected override void OnUpdate() {
            this.EntityManager.RemoveComponent(this.query, typeof(Changed));
        }
    }
}