using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// A custom interface to differentiate a component as a condition resolver
    /// </summary>
    public interface IConditionResolverComponent : IComponentData {
    }
}