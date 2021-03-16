using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateAfter(typeof(PopulateRequiredConditionsSystem))]
    public class IdentifyConditionsToResolveSystem : JobSystemBase {
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            IdentifyJob job = new IdentifyJob() {
                resolverHandle = GetComponentTypeHandle<ConditionResolver>(),
                allPlanners = GetComponentDataFromEntity<GoapPlanner>()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        private struct IdentifyJob : IJobEntityBatch {
            public ComponentTypeHandle<ConditionResolver> resolverHandle;

            [ReadOnly]
            public ComponentDataFromEntity<GoapPlanner> allPlanners;

            [ReadOnly]
            public BufferFromEntity<RequiredCondition> allRequiredConditions;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<ConditionResolver> resolvers = batchInChunk.GetNativeArray(this.resolverHandle);
                for (int i = 0; i < resolvers.Length; ++i) {
                    ConditionResolver resolver = resolvers[i];
                    GoapPlanner planner = this.allPlanners[resolver.plannerEntity];
                    DynamicBuffer<RequiredCondition> requiredConditions = this.allRequiredConditions[resolver.plannerEntity];
                    
                    // When the planner state is RESOLVING_ACTIONS, 'resolved' will be set to false
                    // thereby causing the condition resolver systems to resolve the value
                    resolver.resolved = planner.state != PlannerState.RESOLVING_ACTIONS 
                        && ContainsConditionId(requiredConditions, resolver.conditionId);
                    
                    // Modify
                    resolvers[i] = resolver;
                }
            }

            // TODO Linear search for now. We can optimize this later by using a Bloom Filter.
            private bool ContainsConditionId(in DynamicBuffer<RequiredCondition> requiredConditions,
                ConditionId conditionId) {
                for (int i = 0; i < requiredConditions.Length; ++i) {
                    if (requiredConditions[i].conditionId == conditionId) {
                        // Found a required condition that's equal to the specified one
                        return true;
                    }
                }

                return false;
            }
        }
    }
}