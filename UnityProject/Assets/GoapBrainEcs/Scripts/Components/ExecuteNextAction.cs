using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A tag component that is used to filter entities for execution of next action in the GOAP plan
    /// </summary>
    public struct ExecuteNextAction : IComponentData {
    }
}