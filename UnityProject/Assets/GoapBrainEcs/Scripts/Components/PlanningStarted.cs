using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A tag component that identifies whether or not a planning has started
    /// </summary>
    public struct PlanningStarted : ICleanupComponentData {
    }
}