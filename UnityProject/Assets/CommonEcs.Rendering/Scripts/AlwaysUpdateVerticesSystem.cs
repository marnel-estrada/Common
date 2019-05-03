using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// The UpdateVerticesSystem for sprites in managers that's been set with AlwaysUpdateMesh = true.
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AlwaysUpdateVerticesSystem : UpdateVerticesSystem {
        protected override EntityQuery ResolveQuery() {
            return GetEntityQuery(ComponentType.ReadOnly<Sprite>(),
                ComponentType.ReadOnly<SpriteManager>());
        }

        protected override bool ShouldProcess(ref SpriteManager spriteManager) {
            // Process only managers where AlwaysUpdateMesh = true 
            return spriteManager.AlwaysUpdateMesh;
        }
    }
}