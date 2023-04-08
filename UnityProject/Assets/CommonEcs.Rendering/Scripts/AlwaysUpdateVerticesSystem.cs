using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// The UpdateVerticesSystem for sprites in managers that's been set with AlwaysUpdateMesh = true.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class AlwaysUpdateVerticesSystem : UpdateVerticesSystem {
        protected override EntityQuery ResolveQuery() {
            return GetEntityQuery(ComponentType.ReadOnly<Sprite>(),
                ComponentType.ReadOnly<SpriteManager>());
        }

        protected override bool ShouldProcess(in SpriteManager spriteManager) {
            // Process only managers where AlwaysUpdateMesh = true 
            return spriteManager.AlwaysUpdateMesh;
        }
    }
}