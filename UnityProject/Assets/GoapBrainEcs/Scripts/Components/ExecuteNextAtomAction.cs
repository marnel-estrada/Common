using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A component tag that is used to filter entities that needs to execute the next atom action.
    /// </summary>
    public struct ExecuteNextAtomAction : IComponentData {
    }
}