using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A tag component that marks a PlanExecution entity as failed
    /// We did it this way so we can easily destroy such plan entities as a group
    /// </summary>
    public struct PlanExecutionFailed : IComponentData {
    }
}