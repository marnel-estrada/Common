using Unity.Burst;
using Unity.Burst.Intrinsics;
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
            MoveToNextGoalJob moveToNextGoalJob = new() {
                allAgents = GetComponentLookup<GoapAgent>(true),
                allDebug = GetComponentLookup<DebugEntity>(),
                plannerType = GetComponentTypeHandle<GoapPlanner>()
            };

            return moveToNextGoalJob.ScheduleParallel(this.query, inputDeps);
        }

        [BurstCompile]
        private struct MoveToNextGoalJob : IJobChunk {
            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebug;

            public ComponentTypeHandle<GoapPlanner> plannerType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);
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