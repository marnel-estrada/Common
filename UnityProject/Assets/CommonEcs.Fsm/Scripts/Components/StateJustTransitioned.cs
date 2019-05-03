using System;

using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// A tag component used to determine that a state just transitioned
    /// </summary>
    public struct StateJustTransitioned : IComponentData {
    }
}
