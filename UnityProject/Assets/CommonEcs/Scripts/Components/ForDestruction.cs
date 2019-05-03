using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// This is a tag component used to identify an entity to be destroyed safely in an ECS system
    /// This is for cases when MonoBehaviour want to destroy an Entity but it's not safe to do
    /// outside the context of a system. The MonoBehaviour class will add this component instead.
    /// </summary>
    public struct ForDestruction : IComponentData {
    }
}