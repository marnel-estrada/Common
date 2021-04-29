using Unity.Burst;
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
    public class EndConditionResolversSystem : JobSystemBase {
        private EntityQuery resolversQuery;
        private EntityQuery plannersQuery;

        protected override void OnCreate() {
            this.resolversQuery = GetEntityQuery(typeof(ConditionResolver));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // This can run in parallel
            SetResultsToPlannerBucketJob setResultsJob = new SetResultsToPlannerBucketJob() {
                resolverType = GetComponentTypeHandle<ConditionResolver>(),
                allBuckets = GetBufferFromEntity<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>>()
            };
            JobHandle handle = setResultsJob.ScheduleParallel(this.resolversQuery, 1, inputDeps);

            SetPlannerToResolvingActionsJob setToResolvingActionsJob = new SetPlannerToResolvingActionsJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>()
            };
            handle = setToResolvingActionsJob.ScheduleParallel(this.plannersQuery, 1, handle);

            return handle;
        }

        [BurstCompile]
        private struct SetResultsToPlannerBucketJob : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<ConditionResolver> resolverType;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> allBuckets;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<ConditionResolver> resolvers = batchInChunk.GetNativeArray(this.resolverType);
                for (int i = 0; i < resolvers.Length; ++i) {
                    ConditionResolver resolver = resolvers[i];
                    
                    // Set the value
                    DynamicBuffer<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucket = this.allBuckets[resolver.plannerEntity];
                    bucket[resolver.resultIndex] = DynamicBufferHashMap<ConditionId, bool>.Entry<bool>.Something(resolver.id.hashCode, resolver.result);
                }
            }
        }
        
        [BurstCompile]
        private struct SetPlannerToResolvingActionsJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state == PlanningState.RESOLVING_CONDITIONS) {
                        planner.state = PlanningState.RESOLVING_ACTIONS;
                        
                        // Modify
                        planners[i] = planner;
                    }
                }
            }
        }
    }
}