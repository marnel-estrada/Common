using Unity.Burst;
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
            Job job = new Job() {
                agentType = GetComponentTypeHandle<GoapAgent>(), 
                allPlanners = GetComponentDataFromEntity<GoapPlanner>()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<GoapAgent> agentType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapPlanner> allPlanners;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapAgent> agents = batchInChunk.GetNativeArray(this.agentType);
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