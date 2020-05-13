using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A tag that we can add to exclude a sprite from being included in IdentifySpriteManagerChangedSystem
    /// </summary>
    public readonly struct AlwaysUpdateMesh : IComponentData {
    }
}