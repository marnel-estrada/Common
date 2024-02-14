using CommonEcs;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Common {
    /// <summary>
    /// Handles adding and removal of items into the sweep and prune algorithm.
    /// </summary>
    public partial class SweepPruneSystem : SystemBase {
        private EntityCommandBufferSystem? commandBufferSystem;
        
        private EntityQuery addQuery;
        private EntityQuery removeQuery;
        private EntityQuery updateQuery;

        private SweepPruneContainer container;

        private const int INITIAL_CAPACITY = 8;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            this.addQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SweepPruneEntry>()
                .WithAll<Aabb2Component>().WithNone<Added>().Build(this);

            this.removeQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Added>()
                .WithNone<SweepPruneEntry>()
                .WithNone<Aabb2Component>().Build(this);

            this.updateQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SweepPruneEntry>()
                .WithAll<Aabb2Component>().WithAll<Added>().Build(this);

            this.container = new SweepPruneContainer(INITIAL_CAPACITY, Allocator.Persistent);
        }

        protected override void OnDestroy() {
            this.container.Dispose();
        }

        protected override void OnUpdate() {
        }
        
        [BurstCompile]
        private struct AddJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            }
        }

        // The cleanup component to listen to when adding/removing items from the algorithm
        private readonly struct Added : ICleanupComponentData {
        }
    }
}