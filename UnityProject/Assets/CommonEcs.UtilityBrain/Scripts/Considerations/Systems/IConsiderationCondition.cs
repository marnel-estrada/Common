using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public interface IConsiderationCondition<TComponent> where TComponent : struct, IConsiderationComponent {
        bool IsMet(in Entity agentEntity, in TComponent filterComponent, int indexOfFirstEntityInQuery, int iterIndex);
    }
}