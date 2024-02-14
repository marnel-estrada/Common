using CommonEcs;
using Unity.Entities;
using UnityEngine;

namespace Common {
    /// <summary>
    /// Denotes an entity that it is part of sweep and prune
    /// </summary>
    public struct SweepPruneEntry : IComponentData {
    }

    public class SweepPruneEntryAuthoring : MonoBehaviour {
        private class Baker : Baker<SweepPruneEntryAuthoring> {
            public override void Bake(SweepPruneEntryAuthoring authoring) {
                AddComponent<SweepPruneEntry>(this.GetPrimaryEntity());
            }
        }
    }
}