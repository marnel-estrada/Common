using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Destroys all entities with SetMaterialToSpriteManagersOfLayer components 
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SetSpriteLayerMaterialCleanupSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(SetSpriteLayerMaterial));
        }

        protected override void OnUpdate() {
            // We destroy by query as it is faster
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}