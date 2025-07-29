using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A generic enableable component that marks an entity as "cancelled"
    /// </summary>
    public struct Cancelled : IComponentData, IEnableableComponent {    
    }
}