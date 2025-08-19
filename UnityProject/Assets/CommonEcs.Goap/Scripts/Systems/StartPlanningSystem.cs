using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(ReplanSystem))]
    public partial class StartPlanningSystem : JobSystemBase {
        private EntityQuery plannerQuery;
        private EntityQuery agentsQuery;

        protected override void OnCreate() {
            this.plannerQuery = GetEntityQuery(typeof(GoapPlanner),
                typeof(DynamicBufferHashMap<ConditionId, bool>.Entry));

            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            StartPlanningJob startPlanningJob = new() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                bucketType = GetBufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry>(),
                allAgents = GetComponentLookup<GoapAgent>()
            };
            JobHandle handle = startPlanningJob.ScheduleParallel(this.plannerQuery, inputDeps);

            SetIdleAgentsWithGoalsToPlanningJob setToPlanningJob = new() {
                agentType = GetComponentTypeHandle<GoapAgent>()
            };
            handle = setToPlanningJob.ScheduleParallel(this.agentsQuery, handle);

            return handle;
        }

        [BurstCompile]
        private struct StartPlanningJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry> bucketType;
            
            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);
                BufferAccessor<DynamicBufferHashMap<ConditionId, bool>.Entry> buckets = 
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
                    DynamicBuffer<DynamicBufferHashMap<ConditionId, bool>.Entry> bucket = buckets[i];
                    ConditionsMap.ResetValues(ref bucket);
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
        
        [BurstCompile]
        private struct SetIdleAgentsWithGoalsToPlanningJob : IJobChunk {
            public ComponentTypeHandle<GoapAgent> agentType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while(enumerator.NextEntityIndex(out int i)) {
                    GoapAgent agent = agents[i];
                    
                    if (agent.state == AgentState.IDLE && agent.goals.Count > 0) {
                        // Agent must have started planning
                        agent.state = AgentState.PLANNING;
                        
                        // Modify
                        agents[i] = agent;
                    }
                }
            }
        }
    }
}