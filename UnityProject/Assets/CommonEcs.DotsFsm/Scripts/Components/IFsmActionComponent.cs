using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// A custom component to differentiate components that works as an FSM action.
    /// </summary>
    public interface IFsmActionComponent : IComponentData {       
    }
}