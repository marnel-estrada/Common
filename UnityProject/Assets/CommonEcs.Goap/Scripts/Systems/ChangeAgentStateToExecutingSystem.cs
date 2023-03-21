using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(MoveToNextGoalIfPlanningFailedSystem))]
    public partial class ChangeAgentStateToExecutingSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ChangeStateJob changeStateJob = new ChangeStateJob() {
                agentType = GetComponentTypeHandle<GoapAgent>(), 
                allPlanners = GetComponentLookup<GoapPlanner>()
            };
            
            return changeStateJob.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct ChangeStateJob : IJobChunk {
            public ComponentTypeHandle<GoapAgent> agentType;

            [ReadOnly]
            public ComponentLookup<GoapPlanner> allPlanners;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);
                for (int i = 0; i < agents.Length; ++i) {
                    GoapAgent agent = agents[i];
                    GoapPlanner planner = this.allPlanners[agent.plannerEntity];
                    if (agent.state == AgentState.PLANNING && planner.state == PlanningState.SUCCESS) {
                        // This means that planning has succeeded and agent can proceed to execute
                        // actions
                        agent.state = AgentState.EXECUTING;
                        
                        // Don't forget to reset this. It may retain the previous result of 
                        // success or fail.
                        agent.lastResult = GoapResult.RUNNING;
                        
                        // Also reset the variables needed for action execution
                        agent.currentActionIndex = 0;
                        agent.currentAtomActionIndex = 0;
                        
                        // Modify
                        agents[i] = agent;
                    }
                }
            }
        }
    }
}