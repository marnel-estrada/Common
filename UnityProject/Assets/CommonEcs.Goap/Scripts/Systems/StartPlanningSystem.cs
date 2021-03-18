using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    public class StartPlanningSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            StartPlanningJob job = new StartPlanningJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };

            JobHandle handle = job.ScheduleParallel(this.query, 1, inputDeps);
            
            // Update state of agent
            handle = this.Entities.ForEach(delegate(ref GoapAgent agent) {
                if (agent.state == AgentState.IDLE && agent.goals.Count > 0) {
                    // Agent must have started planning
                    agent.state = AgentState.PLANNING;
                } 
            }).ScheduleParallel(handle);

            return handle;
        }

        private struct StartPlanningJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            
            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];
                    
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    if (agent.state != AgentState.IDLE) {
                        // Maybe the state is currently planning of executing
                        return;
                    }
                
                    if (agent.goals.Count == 0) {
                        // No goals specified yet
                        return;
                    }
                
                    DotsAssert.IsTrue(agent.goalIndex >= 0 && agent.goalIndex < agent.goals.Count);
                    planner.StartPlanning(agent.CurrentGoal);
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
    }
}