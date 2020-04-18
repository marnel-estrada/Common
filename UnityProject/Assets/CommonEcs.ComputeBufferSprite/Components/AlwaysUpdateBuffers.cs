using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// This is a tag component that identifies a sprite as always updating the buffers
    /// This will identify an entity to be excluded in IdentifyDrawInstanceChangedSystem 
    /// </summary>
    public readonly struct AlwaysUpdateBuffers : IComponentData {
    }
}