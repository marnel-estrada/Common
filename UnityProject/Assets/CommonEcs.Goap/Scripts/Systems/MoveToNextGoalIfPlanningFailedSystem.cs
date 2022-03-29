using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(ResolveActionsSystem))]
    public partial class MoveToNextGoalIfPlanningFailedSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                allAgents = GetComponentDataFromEntity<GoapAgent>(true),
                allDebug = GetComponentDataFromEntity<DebugEntity>(),
                plannerType = GetComponentTypeHandle<GoapPlanner>()
            };

            return job.ScheduleParallel(this.query, inputDeps);
        }

        [BurstCompile]
        private struct Job : IJobEntityBatch {
            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentDataFromEntity<DebugEntity> allDebug;

            public ComponentTypeHandle<GoapPlanner> plannerType;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];

                    if (this.allDebug[planner.agentEntity].enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    if (planner.state != PlanningState.FAILED) {
                        // Not failed
                        continue;
                    }

                    GoapAgent agent = this.allAgents[planner.agentEntity];

                    // Agent does not have a goal yet
                    // This also prevents divide by zero
                    if (agent.goals.Count == 0) {
                        continue;
                    }

                    // Move goalIndex to the next
                    planner.goalIndex = (planner.goalIndex + 1) % agent.goals.Count;
                    planner.StartPlanning(agent.goals[planner.goalIndex]);

                    // Modify
                    planners[i] = planner;
                }
            }
        }
    }
}