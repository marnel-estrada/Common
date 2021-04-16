using Unity.Entities;

namespace CommonEcs.Goap {
    public interface IConditionResolverProcess<T> where T : unmanaged, IComponentData {
        bool IsMet(in Entity agentEntity, ref T resolverComponent);
    }
}