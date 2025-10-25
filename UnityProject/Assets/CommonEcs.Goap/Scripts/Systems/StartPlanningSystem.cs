using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(ReplanSystem))]
    public partial class StartPlanningSystem : JobSystemBase {
        private EntityQuery plannerQuery;
        private EntityQuery agentsQuery;

        protected override void OnCreate() {
            this.plannerQuery = GetEntityQuery(typeof(GoapPlanner),
                typeof(ConditionValueMap.Entry));

            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            StartPlanningJob startPlanningJob = new() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                bucketType = GetBufferTypeHandle<ConditionValueMap.Entry>(),
                allAgents = GetComponentLookup<GoapAgent>()
            };
            inputDeps = startPlanningJob.ScheduleParallel(this.plannerQuery, inputDeps);

            SetIdleAgentsWithGoalsToPlanningJob setToPlanningJob = new() {
                entityType = GetEntityTypeHandle(),
                agentType = GetComponentTypeHandle<GoapAgent>(),
                debugEntityType = GetComponentTypeHandle<DebugEntity>()
            };
            inputDeps = setToPlanningJob.ScheduleParallel(this.agentsQuery, inputDeps);

            return inputDeps;
        }

        [BurstCompile]
        private struct StartPlanningJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<ConditionValueMap.Entry> bucketType;
            
            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);
                BufferAccessor<ConditionValueMap.Entry> buckets = 
                    chunk.GetBufferAccessor(ref this.bucketType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while(enumerator.NextEntityIndex(out int i)) {
                    GoapPlanner planner = planners[i];
                    
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    if (agent.state != AgentState.IDLE) {
                        // Maybe the state is currently planning of executing
                        continue;
                    }
                
                    if (agent.goals.Count == 0) {
                        // No goals specified yet
                        continue;
                    }
                
                    DotsAssert.IsTrue(planner.goalIndex >= 0 && planner.goalIndex < agent.goals.Count);
                    planner.StartPlanning(agent.GetGoal(planner.goalIndex));
                    
                    // Reset the condition values
                    DynamicBuffer<ConditionValueMap.Entry> bucket = buckets[i];
                    ConditionsMap.ResetValues(ref bucket);
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
        
        [BurstCompile]
        private struct SetIdleAgentsWithGoalsToPlanningJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            public ComponentTypeHandle<GoapAgent> agentType;

            [ReadOnly]
            public ComponentTypeHandle<DebugEntity> debugEntityType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);
                NativeArray<DebugEntity> debugEntities = chunk.GetNativeArray(ref this.debugEntityType);
                
                DotsAssert.IsFalse(useEnabledMask);
                for (int i = 0; i < chunk.Count; i++) {
                    GoapAgent agent = agents[i];

                    if (agent.state != AgentState.IDLE || agent.goals.Count <= 0) {
                        continue;
                    }

                    // Agent must have started planning
                    agent.state = AgentState.PLANNING;

                    DebugEntity debugEntity = debugEntities[i];
                    if (debugEntity.enabled) {
                        Debug.Log($"Agent {entities[i].Index} has been set to AgentState.PLANNING.");
                    }
                        
                    // Modify
                    agents[i] = agent;
                }
            }
        }
    }
}