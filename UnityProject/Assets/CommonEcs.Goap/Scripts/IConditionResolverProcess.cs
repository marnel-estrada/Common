using Unity.Entities;

namespace CommonEcs.Goap {
    public interface IConditionResolverProcess<T> where T : unmanaged, IConditionResolverComponent {
        bool IsMet(in Entity agentEntity, ref T resolverComponent);
    }
}