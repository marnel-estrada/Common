using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(PopulateRequiredConditionsSystem))]
    public partial class IdentifyConditionsToResolveSystem : JobSystemBase {
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            IdentifyJob job = new() {
                resolverHandle = GetComponentTypeHandle<ConditionResolver>(),
                resolvedEnableableType = GetComponentTypeHandle<ConditionResolver.Resolved>(),
                allPlanners = GetComponentLookup<GoapPlanner>(),
                allRequiredConditions = GetBufferLookup<RequiredCondition>()
            };
            
            return job.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct IdentifyJob : IJobChunk {
            public ComponentTypeHandle<ConditionResolver> resolverHandle;
            public ComponentTypeHandle<ConditionResolver.Resolved> resolvedEnableableType;

            [ReadOnly]
            public ComponentLookup<GoapPlanner> allPlanners;

            [ReadOnly]
            public BufferLookup<RequiredCondition> allRequiredConditions;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ConditionResolver> resolvers = chunk.GetNativeArray(ref this.resolverHandle);
                for (int i = 0; i < resolvers.Length; ++i) {
                    ConditionResolver resolver = resolvers[i];
                    GoapPlanner planner = this.allPlanners[resolver.plannerEntity];
                    DynamicBuffer<RequiredCondition> requiredConditions = this.allRequiredConditions[resolver.plannerEntity];
                    
                    // Set whether the condition is resolved or not
                    bool isResolved = !(planner.state == PlanningState.RESOLVING_CONDITIONS 
                        && ContainsConditionId(requiredConditions, resolver.conditionId));
                    chunk.SetComponentEnabled(ref this.resolvedEnableableType, i, isResolved);
                    
                    // Modify
                    resolvers[i] = resolver;
                }
            }

            // TODO Linear search for now. We can optimize this later by using a Bloom Filter.
            private static bool ContainsConditionId(in DynamicBuffer<RequiredCondition> requiredConditions,
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