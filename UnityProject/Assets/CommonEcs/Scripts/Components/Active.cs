using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A tag component that identifies an entity as active
    /// We can use this as the equivalent of GameObject.isActive
    /// </summary>
    public struct Active : IComponentData, IEnableableComponent {    
    }
}