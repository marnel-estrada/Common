using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Common interface that will compute a utility.
    /// This is like the IConsideration implementation in OOP UtilityBrain.
    /// </summary>
    public interface IConsiderationProcess<T> where T : unmanaged, IConsiderationComponent {
        UtilityValue ComputeUtility(in Entity agentEntity, in T filterComponent);
    }
}