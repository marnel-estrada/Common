using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateAfter(typeof(ResolveActionsSystem))]
    public class MoveToNextGoalIfPlanningFailedSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                allAgents = GetComponentDataFromEntity<GoapAgent>(true),
                plannerType = GetComponentTypeHandle<GoapPlanner>()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public ComponentTypeHandle<GoapPlanner> plannerType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlanningState.FAILED) {
                        // Not failed
                        continue;
                    }
                    
                    // Move goalIndex to the next
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    planner.goalIndex = (planner.goalIndex + 1) % agent.goals.Count;
                    planner.StartPlanning(agent.goals[planner.goalIndex]);
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
    }
}