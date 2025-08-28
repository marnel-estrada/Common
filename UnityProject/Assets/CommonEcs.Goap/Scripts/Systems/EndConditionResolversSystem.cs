using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Condition resolvers system must run before this system.
    /// This will set to results to the conditionsMap of the planner.
    /// This runs in a single thread. There's no way around it.
    /// </summary>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyConditionsToResolveSystem))]
    public partial class EndConditionResolversSystem : JobSystemBase {
        private EntityQuery resolversQuery;
        private EntityQuery plannersQuery;

        protected override void OnCreate() {
            this.resolversQuery = GetEntityQuery(typeof(ConditionResolver));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // This can run in parallel
            SetResultsToPlannerBucketJob setResultsJob = new() {
                resolverType = GetComponentTypeHandle<ConditionResolver>(),
                allBuckets = GetBufferLookup<ConditionValueMap.Entry>()
            };
            JobHandle handle = setResultsJob.ScheduleParallel(this.resolversQuery, inputDeps);

            SetPlannerToResolvingActionsJob setToResolvingActionsJob = new() {
                plannerType = GetComponentTypeHandle<GoapPlanner>()
            };
            handle = setToResolvingActionsJob.ScheduleParallel(this.plannersQuery, handle);

            return handle;
        }

        [BurstCompile]
        private struct SetResultsToPlannerBucketJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ConditionResolver> resolverType;

            [NativeDisableParallelForRestriction]
            public BufferLookup<ConditionValueMap.Entry> allBuckets;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ConditionResolver> resolvers = chunk.GetNativeArray(ref this.resolverType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    ConditionResolver resolver = resolvers[i];
                    
                    // Set the value
                    DynamicBuffer<ConditionValueMap.Entry> bucket = this.allBuckets[resolver.plannerEntity];
                    bucket[resolver.resultIndex] = ConditionValueMap.Entry.Something(resolver.conditionId.hashCode, resolver.result);
                }
            }
        }
        
        [BurstCompile]
        private struct SetPlannerToResolvingActionsJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlanningState.RESOLVING_CONDITIONS) {
                        continue;
                    }

                    planner.state = PlanningState.RESOLVING_ACTIONS;
                    planners[i] = planner; // Modify
                }
            }
        }
    }
}